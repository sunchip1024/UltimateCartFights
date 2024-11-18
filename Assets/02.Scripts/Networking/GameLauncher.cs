using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using UltimateCartFights.UI;
using UltimateCartFights.Utility;
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

            // FSM 실행
            stateManager.Start();
        }

        private void Update() {
            stateManager.Update();
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

        public static SessionInfo GetSessionInfo() {
            return Runner.SessionInfo;
        }

        public static bool IsHost() {
            if (Runner == null) return false;
            return Runner.IsServer;
        } 

        #endregion

        #region Network Method

        public static async new void Open() {
            try {
                stateManager.StopState();
                await FusionSocket.Open();
                stateManager.StartState(STATE.LOADING_LOBBY);
            } catch(NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static async new void JoinLobby() {
            try {
                stateManager.StopState();
                await FusionSocket.JoinLobby();
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static async new void CreateRoom(RoomInfo room) {
            try {
                stateManager.StopState();
                await FusionSocket.CreateRoom(room);
                stateManager.StartState(STATE.ROOM);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static async new void JoinRoom(RoomInfo room) {
            try {
                stateManager.StopState();
                await FusionSocket.JoinRoom(room);
                stateManager.StartState(STATE.ROOM);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static void StartLoading() {
            try {
                stateManager.StopState();
                stateManager.StartState(STATE.LOADING_GAME);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static void StartGame() {
            try {
                stateManager.StopState();
                stateManager.StartState(STATE.GAME);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static void ShowResult() {
            try {
                stateManager.StopState();
                stateManager.StartState(STATE.RESULT);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static void ReturnRoom() {
            try {
                stateManager.StopState();
                stateManager.StartState(STATE.ROOM);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        public static async new void Close() {
            try {
                stateManager.StopState();
                await FusionSocket.Close();
                stateManager.StartState(STATE.CLOSED);
            } catch (NetworkException e) {
                stateManager.Abort();
                PopupUI.Instance.OpenError(GetShutdownMessage(e.ShutdownReason));
            }
        }

        #endregion

        #region Scene Loading Method

        public static void LoadGame() {
            sceneManager.LoadScene(SCENE.GAME);
        }

        public static void LoadRoom() {
            sceneManager.LoadScene(SCENE.ROOM);
        }

        #endregion

        #region Connection Event Method

        public override void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
            base.OnConnectRequest(runner, request, token);

            NetworkSceneInfo scene;

            // 현재 진행 중인 씬 정보 불러오기에 실패하면 연결하지 않는다
            if (runner.TryGetSceneInfo(out scene) == false)
                request.Refuse();
            // 현재 로딩된 씬(= 게임 씬)이 있다면 연결하지 않는다
            else if(scene.SceneCount > 0)
                request.Refuse();
            // 현재 게임 룸에 있다면 연결한다
            else
                request.Accept();
        }

        public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
            base.OnConnectFailed(runner, remoteAddress, reason);
            Open();
        }

        // 네트워크가 종료될 때 실행되는 함수
        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            base.OnShutdown(runner, shutdownReason);

            // 정상적인 종료가 아니라면 오류 메세지를 띄운다
            if (shutdownReason != ShutdownReason.Ok)
                PopupUI.Instance.OpenError(GetShutdownMessage(shutdownReason));
        }

        // 네트워크 종료 이유 메세지를 반환한다
        private static string GetShutdownMessage(ShutdownReason shutdownReason) {
            switch(shutdownReason) {
                case ShutdownReason.GameClosed:
                    return "방과의 접속이 끊겼습니다!";

                case ShutdownReason.GameNotFound:
                    return "해당 방이 없습니다!";

                case ShutdownReason.GameIsFull:
                    return "인원이 꽉 찼습니다!";

                case ShutdownReason.ConnectionRefused:
                    return "이미 게임을 시작했습니다!";

                default:
                    return "오류가 발생했습니다!";
            }
        }

        #endregion

        #region Lobby Event Method

        public static List<SessionInfo> sessions = new List<SessionInfo>();

        public override void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
            sessions = sessionList;

            if(isFirstSessionUpdate) {
                stateManager.StopState();

                PanelUI.Instance.RefreshRoomList();
                isFirstSessionUpdate = false;

                stateManager.StartState(STATE.LOBBY);
            }
        }

        #endregion

        #region Room Event Method

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
            if (!runner.IsServer) return;

            runner.Spawn(ResourceManager.Instance.Client, Vector3.zero, Quaternion.identity, player);
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
            if (!runner.IsServer) return;

            ClientPlayer client = ClientPlayer.RemovePlayer(player);
            runner.Despawn(client.Object);
        }

        #endregion
    }
}