using UltimateCartFights.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UltimateCartFights.Utility;

namespace UltimateCartFights.UI {
    public class PopupUI : MonoBehaviour {

        #region Popup UI Singleton

        public static PopupUI Instance { get; private set; } = null;

        private void Awake() {
            if (Instance) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Error Popup UI

        [Header("Error Popup UI")]
        [SerializeField] private GameObject errorPopup;
        [SerializeField] private TMP_Text errorText;

        public void OpenError(string errorMessage) {
            CloseRoom();

            errorText.text = errorMessage;
            errorPopup.SetActive(true);
        }

        public void CloseError() {
            errorPopup.SetActive(false);
        }

        #endregion

        #region Room Creation Popup UI

        [Header("Room Popup UI")]
        [SerializeField] private GameObject roomPopup;
        [SerializeField] private TMP_InputField roomName;
        [SerializeField] private TMP_Text maxPlayerText;
        [SerializeField] private Slider maxPlayer;
        [SerializeField] private Button submit;

        public void OpenRoom() {
            roomName.text = string.Empty;

            maxPlayerText.text = RoomInfo.MAX_PLAYER.ToString();
            maxPlayer.value = RoomInfo.MAX_PLAYER;

            submit.interactable = false;

            roomPopup.SetActive(true);
        }

        public void CloseRoom() {
            roomPopup.SetActive(false);
        }
        
        public void OnModifyRoomName() {
            submit.interactable = !roomName.text.IsNullOrEmpty();
        }

        public void OnModifyMaxPlayer() {
            maxPlayerText.text = ((int) maxPlayer.value).ToString();
        }

        public void OnSubmitRoom() {
            submit.interactable = false;
            CloseRoom();

            RoomInfo room = new RoomInfo(
                string.Empty, 
                roomName.text, 
                (int) maxPlayer.value, 
                ClientInfo.Nickname
            );

            GameLauncher.CreateRoom(room);
        }

        #endregion
    }
}