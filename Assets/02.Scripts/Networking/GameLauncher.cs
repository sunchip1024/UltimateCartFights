using Fusion;
using System.Collections;
using System.Collections.Generic;
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

            // Fusion ���� ���� �׽�Ʈ
            Open();
            JoinLobby();
            Close();
        }

        #endregion

        #region Network Static Method

        public static new void Open() {
            try {
                FusionSocket.Open();
            } catch {
                stateManager.Abort();
            }
        }

        public static new void JoinLobby() {
            try {
                FusionSocket.JoinLobby();
            } catch {
                stateManager.Abort();
            }
        }

        public static async new void Close() {
            try {
                await FusionSocket.Close();
            } catch {
                stateManager.Abort();
            }
        }

        #endregion
    }
}