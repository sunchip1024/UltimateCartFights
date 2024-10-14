using Fusion;
using System;
using System.Collections.Generic;
using UltimateCartFights.UI;
using UnityEngine;

namespace UltimateCartFights.Network {
    public class GameLauncher : FusionSocket {
        
        #region Unity Basic Method

        private void Start() {
            // ���� ȯ�� ����
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            DontDestroyOnLoad(this.gameObject);

            // �κ� ��� ������ �̵�
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        #endregion

        #region Network State Method

        public static bool IsNetworked() {
            switch(stateManager.State) {
                case STATE.CLOSED:
                case STATE.LOADING_LOBBY:
                    return false;

                default:
                    return true;
            } 
        }

        #endregion

        #region Network Method

        public static async new void Open() {
            try {
                await FusionSocket.Open();
            } catch(Exception e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(e.Message);
            }
        }

        public static async new void JoinLobby() {
            try {
                await FusionSocket.JoinLobby();
            } catch(Exception e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(e.Message);
            }
        }

        public static async new void CreateRoom(RoomInfo room) {
            try {
                await FusionSocket.CreateRoom(room);
            } catch(Exception e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(e.Message);
            }
        }

        public static async new void JoinRoom(RoomInfo room) {
            try {
                await FusionSocket.JoinRoom(room);
            } catch (Exception e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(e.Message);
            }
        }

        public static async new void Close() {
            try {
                await FusionSocket.Close();
            } catch(Exception e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(e.Message);
            }
        }

        #endregion

        #region Connection Event Method

        // ��Ʈ��ũ�� ����� �� ����Ǵ� �Լ�
        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            base.OnShutdown(runner, shutdownReason);

            // �������� ���ᰡ �ƴ϶�� ���� �޼����� ����
            if (shutdownReason != ShutdownReason.Ok) {
                PopupUI.Instance.OpenError(GetShutdownMessage(shutdownReason));
                Close();
            }
        }

        // ��Ʈ��ũ ���� ���� �޼����� ��ȯ�Ѵ�
        private string GetShutdownMessage(ShutdownReason shutdownReason) {
            switch(shutdownReason) {
                case ShutdownReason.GameClosed:
                    return "����� ������ ������ϴ�!";

                case ShutdownReason.GameNotFound:
                    return "�ش� ���� �����ϴ�!";

                case ShutdownReason.GameIsFull:
                    return "�ο��� �� á���ϴ�!";

                default:
                    return "������ �߻��߽��ϴ�!";
            }
        }

        #endregion

        #region Lobby Event Method

        public static List<SessionInfo> sessions = new List<SessionInfo>();

        public override void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
            sessions = sessionList;
            base.OnSessionListUpdated(runner, sessionList);
        }

        #endregion
    }
}