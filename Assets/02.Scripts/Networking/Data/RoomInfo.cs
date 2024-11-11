using UnityEngine;

namespace UltimateCartFights.Network {
    public class RoomInfo {

        public const int MAX_PLAYER = 6;

        // 해당 방의 고유 ID (중복 X)
        public string RoomID { get; private set; }

        // 해당 방의 이름 (중복 가능)
        public string RoomName { get; private set; }

        // 해당 방의 최대 참가 인원
        public int MaxPlayer { get; private set; }

        // 해당 방의 방장 닉네임
        public string HostNickname { get; private set; }

        public RoomInfo(string roomID, string roomName, int maxPlayer, string hostNickname) {
            RoomID = roomID;
            RoomName = roomName;
            MaxPlayer = maxPlayer;
            HostNickname = hostNickname;
        }
    }
}