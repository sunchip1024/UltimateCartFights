using AYellowpaper.SerializedCollections;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltimateCartFights.Network;
using UltimateCartFights.Utility;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace UltimateCartFights.UI {
    public class LobbyPanelUI : MonoBehaviour {

        #region Unity Basic Method

        private void Awake() {
            nickname.text = ClientInfo.Nickname;
        }

        #endregion

        #region Nickname Section

        [Header("Nickname Section")]
        [SerializeField] private TMP_InputField nickname;

        public void OnModifyNickname() {
            if (!nickname.text.Trim().IsNullOrEmpty())
                ClientInfo.Nickname = nickname.text.Trim();

            nickname.text = ClientInfo.Nickname;
            SetActiveRoom(true);
        }

        #endregion

        #region Mode Section

        [Header("Mode Section")]
        [SerializeField] private Button roomCreation;

        public void SetActiveRoom(bool active) {
            // 방 생성 버튼을 활성화하기 위해서는 최소 두 조건을 만족해야 한다
            // 1) 닉네임이 공백 제외 한 글자 이상으로 정해져야 한다
            // 2) Fusion 서버에 접속한 상태여야 한다
            active &= !ClientInfo.Nickname.IsNullOrEmpty();
            active &= GameLauncher.IsNetworked();
            roomCreation.interactable = active;
        }

        #endregion
        
        #region RoomList Section

        public enum Screen { LOADING, EMPTY, ROOM_LIST }

        [Header("RoomList Section")]
        [SerializedDictionary("Screen", "UI")]
        [SerializeField] private SerializedDictionary<Screen, GameObject> screens;
        [SerializeField] private GameObject roomContainer;
        [SerializeField] private RoomItem roomItem;

        private GameObject currentScreen = null;
        private List<RoomItem> roomItems = new List<RoomItem>();

        public void SetScreen(Screen type) {
            if (currentScreen != null)
                currentScreen.SetActive(false);

            currentScreen = screens[type];
            currentScreen.SetActive(true);
        }

        public void CreateRoomList(List<SessionInfo> sessions) {
            ClearRoomList();

            if(sessions.Count == 0) {
                SetScreen(Screen.EMPTY);
                return;
            }

            foreach(SessionInfo session in sessions)
                CreateRoomItem(session);

            SetScreen(Screen.ROOM_LIST);
        }

        private void ClearRoomList() {
            foreach (RoomItem room in roomItems)
                Destroy(room.gameObject);

            roomItems.Clear();
        }

        private void CreateRoomItem(SessionInfo session) {
            RoomItem room = Instantiate(roomItem);
            room.SetRoom(session);
            room.SetParent(roomContainer.transform);

            roomItems.Add(room);
        }

        private bool IsValidRoom(SessionInfo session) {
            if (!session.Properties.ContainsKey("RoomName")) return false;
            if (!session.Properties.ContainsKey("HostNickname")) return false;
            return true;
        }

        #endregion

        #region Search Section

        [Header("Search Section")]
        [SerializeField] private TMP_InputField search;

        public void SearchRoomList() {
            if(search.text.IsNullOrEmpty()) {
                RefreshRoomList();
                return;
            }

            List<SessionInfo> result = (from session in GameLauncher.sessions
                                        where IsValidRoom(session) 
                                        && ((string) session.Properties["RoomName"]).Contains(search.text)
                                        select session).ToList();
            
            CreateRoomList(result);
        }

        public void RefreshRoomList() {
            search.text = string.Empty;

            List<SessionInfo> result = (from session in GameLauncher.sessions
                                        where IsValidRoom(session)
                                        select session).ToList();

            CreateRoomList(result);
        }

        #endregion
    }
}