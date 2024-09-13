using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateCartFights.Network {
    public class GameLauncher : FusionSocket {

        #region Unity Basic Method

        private void Start() {
            // 게임 환경 설정
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            DontDestroyOnLoad(this.gameObject);

            // 로비 대기 씬으로 이동
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

            // Fusion 서버 접속 테스트
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