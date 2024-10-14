using Fusion;
using UltimateCartFights.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UltimateCartFights.Utility;
using WebSocketSharp;

namespace UltimateCartFights.UI {
    public class RoomItem : MonoBehaviour {

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text host;
        [SerializeField] private TMP_Text capacity;
        [SerializeField] private Button entrance;

        public RoomInfo roomInfo { get; private set; }

        public void SetRoom(SessionInfo session) {
            roomInfo = new RoomInfo(
                session.Name,
                (string) session.Properties["RoomName"],
                session.MaxPlayers,
                (string) session.Properties["HostNickname"]
            );

            title.text = roomInfo.RoomName;
            host.text = roomInfo.HostNickname;
            capacity.text = string.Format("{0} / {1}", session.PlayerCount, roomInfo.MaxPlayer);

            bool active = (session.PlayerCount < roomInfo.MaxPlayer);
            active &= GameLauncher.IsNetworked();
            active &= !ClientInfo.Nickname.IsNullOrEmpty();
            entrance.interactable = active;
        }

        public void SetParent(Transform parent) {
            transform.SetParent(parent);
            GetComponent<RectTransform>().localScale = Vector3.one;
        }

        public void JoinRoom() {
            entrance.interactable = false;
            GameLauncher.JoinRoom(roomInfo);
        }
    }
}