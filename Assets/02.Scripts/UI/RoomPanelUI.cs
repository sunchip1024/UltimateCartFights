using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltimateCartFights.Network;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace UltimateCartFights.UI {
    public class RoomPanelUI : MonoBehaviour {

        #region Player Section

        [Header("Player Section")]
        [SerializeField] private List<PlayerUI> PlayerUIs;

        private void UpdateProfile(int playerID) {
            PlayerUI playerUI = PlayerUIs[playerID];
            ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.PlayerID == playerID);

            if (playerID >= GameLauncher.GetSessionInfo().MaxPlayers)
                playerUI.SetBlocked();
            else if (client == null)
                playerUI.SetEmpty();
            else
                playerUI.SetPlayerInfo((string) client.Nickname, client.Character, client.IsReady);
        }

        private void UpdateProfile() {
            for (int playerID = 0; playerID < RoomInfo.MAX_PLAYER; playerID++)
                UpdateProfile(playerID);
        }

        #endregion

        #region Chat Session

        [Header("Chat Session")]
        [SerializeField] private TMP_InputField ChatInput;
        [SerializeField] private TMP_Text ChatText;

        public void OnSendMessage() => OnSubmitMessage(ChatInput.text);

        private void OnSubmitMessage(string message) {
            if (message.IsNullOrEmpty()) return;

            ChatNetwork.SendChat(message, ChatType.GENERAL);
            ChatInput.text = string.Empty;
            ChatInput.ActivateInputField();
        }

        private void OnGetMessage(string message, ChatType type) {
            string color;

            switch(type) {
                case ChatType.SYSTEM:
                    color = "red";
                    break;

                case ChatType.GENERAL:
                    color = "black";
                    break;

                default:
                    color = "white";
                    return;
            }

            ChatText.text += string.Format("<color={0}>{1}</color>\n", color, message);
        }

        #endregion

        #region Character Section

        [Header("Character Section")]
        [SerializeField] private List<CharacterSelectionUI> CharacterTypes;

        private int currentCharacter = 0;

        private void InitializeCharacter() {
            for(int type = 0; type < CharacterTypes.Count; type++) {
                CharacterSelectionUI characterUI = CharacterTypes[type];
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.Character == type);

                characterUI.Initialize(type);
                characterUI.gameObject.SetActive(false);
                characterUI.SetBlock(client != null);
            }

            CharacterTypes[currentCharacter].gameObject.SetActive(true);
        }

        private void UpdateCharacter() {
            for (int type = 0; type < CharacterTypes.Count; type++) {
                CharacterSelectionUI characterUI = CharacterTypes[type];
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.Character == type);
                characterUI.SetBlock(client != null);
            }
        }

        public void OnNextCharacter() {
            // 현재 캐릭터 UI를 숨긴다
            CharacterTypes[currentCharacter].gameObject.SetActive(false);

            // 다음 캐릭터 번호를 계산한다 (현재 5번인 경우 다음 번호는 0번이 된다)
            currentCharacter = (currentCharacter + 1) % CharacterTypes.Count;

            // 다음 캐릭터 UI를 표시한다
            CharacterTypes[currentCharacter].gameObject.SetActive(true);
        }

        public void OnPrevCharacter() {
            // 현재 캐릭터 UI를 숨긴다
            CharacterTypes[currentCharacter].gameObject.SetActive(false);

            // 이전 캐릭터 번호를 계산한다 (현재 0번인 경우 이전 번호는 5번이 된다)
            currentCharacter = (CharacterTypes.Count + currentCharacter - 1) % CharacterTypes.Count;

            // 이전 캐릭터 UI를 표시한다
            CharacterTypes[currentCharacter].gameObject.SetActive(true);
        }

        #endregion

        #region Color Section

        [Header("Color Section")]
        [SerializeField] private List<ColorSelectionUI> ColorTypes;

        private void InitializeColor() {
            for(int color = 0; color < ColorTypes.Count; color++) {
                ColorSelectionUI colorUI = ColorTypes[color];
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault (x => x.CartColor == color);

                colorUI.Initialize(color);
                colorUI.SetBlock(client != null);
            }
        }

        private void UpdateColor() {
            for (int color = 0; color < ColorTypes.Count; color++) {
                ColorSelectionUI colorUI = ColorTypes[color];
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.CartColor == color);
                
                colorUI.SetBlock(client != null);
            }
        }

        #endregion

        #region Ready Section

        [Header("Ready Section")]

        [SerializeField] private Button ReadyButton;
        [SerializeField] private Button StartButton;

        [SerializeField] private Image ReadyImage;
        [SerializeField] private Image StartImage;
        [SerializeField] private TMP_Text StartText;

        [SerializeField] private Sprite WaitSprite;
        [SerializeField] private Sprite ReadySprite;
        [SerializeField] private Sprite StartSprite;

        public void OnReady() {
            if (ClientPlayer.Local == null) return;

            ClientPlayer.Local.RPC_SetReady(!ClientPlayer.Local.IsReady);
        }

        public void OnStart() {
            StartButton.interactable = false;

            Debug.Log("[ * Debug * ] Game Start!");
        }

        private void UpdateReady() {
            if (ClientPlayer.Local == null) return;

            if(ClientPlayer.Local.PlayerID == 0) {
                StartImage.sprite = ClientPlayer.CanStartGame() ? StartSprite : WaitSprite;
                StartText.gameObject.SetActive(ClientPlayer.CanStartGame());
                StartButton.interactable = ClientPlayer.CanStartGame();
            } else {
                ReadyImage.sprite = ClientPlayer.CanReady() ? ReadySprite : WaitSprite;
                ReadyButton.interactable = ClientPlayer.CanReady();
            }
        }

        #endregion

        #region Others

        public void OnClickLeaveRoom() => GameLauncher.Open();

        public void InitializeRoom() {
            // Player UI 초기화
            UpdateProfile();

            // 채팅 UI 초기화
            ChatText.text = string.Empty;

            // Character UI 초기화
            InitializeCharacter();

            // Color UI 초기화
            InitializeColor();

            // 준비 버튼 UI 초기화
            StartButton.gameObject.SetActive(GameLauncher.IsHost());
            ReadyButton.gameObject.SetActive(!GameLauncher.IsHost());
            UpdateReady();

            // 이벤트 리스너 추가
            ClientPlayer.PlayerUpdated.AddListener(UpdateProfile);
            ClientPlayer.PlayerUpdated.AddListener(UpdateCharacter);
            ClientPlayer.PlayerUpdated.AddListener(UpdateColor);
            ClientPlayer.PlayerUpdated.AddListener(UpdateReady);

            ChatNetwork.GetMessage.AddListener(OnGetMessage);
            ChatInput.onSubmit.AddListener(OnSubmitMessage);
        }

        public void LeaveRoom() {
            // 이벤트 리스너 제거
            ClientPlayer.PlayerUpdated.RemoveAllListeners();
            
            ChatNetwork.GetMessage.RemoveListener(OnGetMessage);
            ChatInput.onSubmit.RemoveListener(OnSubmitMessage);
        }

        #endregion
    }
}