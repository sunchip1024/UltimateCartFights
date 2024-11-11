using UnityEngine;

namespace UltimateCartFights.Network {
    public class RoomInfo {

        public const int MAX_PLAYER = 6;

        // �ش� ���� ���� ID (�ߺ� X)
        public string RoomID { get; private set; }

        // �ش� ���� �̸� (�ߺ� ����)
        public string RoomName { get; private set; }

        // �ش� ���� �ִ� ���� �ο�
        public int MaxPlayer { get; private set; }

        // �ش� ���� ���� �г���
        public string HostNickname { get; private set; }

        public RoomInfo(string roomID, string roomName, int maxPlayer, string hostNickname) {
            RoomID = roomID;
            RoomName = roomName;
            MaxPlayer = maxPlayer;
            HostNickname = hostNickname;
        }
    }
}