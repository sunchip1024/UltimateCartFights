using AYellowpaper.SerializedCollections;
using TMPro;
using UltimateCartFights.Network;
using UnityEngine;

namespace UltimateCartFights.UI {
    public class PanelUI : MonoBehaviour {

        #region Panel UI Singleton

        public static PanelUI Instance { get; private set; } = null;

        private void Awake() {
            if (Instance) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        // 백그라운드 검은색 Letterbox UI 메서드
        #region Letterbox UI

        [Header("Letterbox")]
        [SerializeField] private GameObject letterbox;

        private void SetLetterbox(Panel type) {
            switch (type) {
                case Panel.INTRO:
                case Panel.LOBBY:
                case Panel.ROOM:
                case Panel.LOADING:
                case Panel.FADE:
                    letterbox.SetActive(true);
                    break;

                default:
                    letterbox.SetActive(false);
                    break;
            }
        }

        #endregion

        // 패널 기본 UI 메서드
        #region Panel UI

        public enum Panel { INTRO, LOBBY, ROOM, LOADING, GAME, RESULT, FADE }

        [Header("Panel")]
        [SerializedDictionary("Network State", "Panel UI")]
        [SerializeField] private SerializedDictionary<Panel, GameObject> panels;

        private GameObject currentPanel = null;

        public void SetPanel(Panel type) {
            if(currentPanel != null)
                currentPanel.SetActive(false);

            currentPanel = panels[type];
            currentPanel.SetActive(true);

            SetLetterbox(type);
        }

        #endregion

        // 인트로 패널 UI 메서드
        #region Intro Panel UI

        // 게임 시작 버튼 클릭 시 Photon Fusion Cloud Server에 접속한다
        public void OnStart() => GameLauncher.Open();

        #endregion

        // 로비 패널 UI 메서드
        #region Lobby Panel UI

        [Header("Lobby Panel UI")]
        [SerializeField] private LobbyPanelUI LobbyUI;

        public void SetLoading() {
            LobbyUI.SetActiveRoom(false);
            LobbyUI.SetScreen(LobbyPanelUI.Screen.LOADING);
        }

        public void SetLobby() {
            LobbyUI.SetActiveRoom(true);
        }

        public void RefreshRoomList() {
            LobbyUI.RefreshRoomList();
        }

        #endregion

        // 게임룸 패널 UI 메서드
        #region Room Panel UI

        [Header("Room Panel UI")]
        [SerializeField] private RoomPanelUI RoomUI;

        public void InitializeRoom() => RoomUI.InitializeRoom();

        public void LeaveRoom() => RoomUI.LeaveRoom();

        #endregion

        // 로딩 패널 UI 메서드
        #region Loading Panel UI

        [Header("Loading Panel UI")]
        [SerializeField] private LoadingPanelUI LoadingUI;

        public void InitializeLoading() {
            LoadingUI.Initialize();
        }

        public void DisableLoading() {
            LoadingUI.Disabled();
        }

        public void SetLoadingProgress(float progress) {
            LoadingUI.SetLoadingProgress(progress);
        }

        #endregion

        // 게임 패널 UI 메서드
        #region Game Panel UI

        [Header("Game Panel UI")]
        [SerializeField] private GamePanelUI GameUI;

        public void InitializeGame() => GameUI.InitializeGameUI();

        public void UpdateGame() => GameUI.UpdateGameUI();

        public void OnGetDamaged(int playerID, float damage) => GameUI.OnGetDamaged(playerID, damage);

        public void OnKnockout(int playerID) => GameUI.OnKnockout(playerID);

        #endregion

        // 결과 패널 UI 메서드
        #region Result Panel UI

        [Header("Result Panel UI")]
        [SerializeField] private TMP_Text winnerText;

        public void SetWinner(string winner) {
            winnerText.text = winner;
        }

        #endregion
    }
}