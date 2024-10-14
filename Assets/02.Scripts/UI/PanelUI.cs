using AYellowpaper.SerializedCollections;
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

        // ��׶��� ������ Letterbox UI �޼���
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

        // �г� �⺻ UI �޼���
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

        // ��Ʈ�� �г� UI �޼���
        #region Intro Panel UI

        // ���� ���� ��ư Ŭ�� �� Photon Fusion Cloud Server�� �����Ѵ�
        public void OnStart() => GameLauncher.Open();

        #endregion

        // �κ� �г� UI �޼���
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
    }
}