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
            // ���� ȯ�� ����
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            DontDestroyOnLoad(this.gameObject);

            // �κ� ��� ������ �̵�
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

            // FSM ����
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

            // ���� ���� ���� �� ���� �ҷ����⿡ �����ϸ� �������� �ʴ´�
            if (runner.TryGetSceneInfo(out scene) == false)
                request.Refuse();
            // ���� �ε��� ��(= ���� ��)�� �ִٸ� �������� �ʴ´�
            else if(scene.SceneCount > 0)
                request.Refuse();
            // ���� ���� �뿡 �ִٸ� �����Ѵ�
            else
                request.Accept();
        }

        public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
            base.OnConnectFailed(runner, remoteAddress, reason);
            Open();
        }

        // ��Ʈ��ũ�� ����� �� ����Ǵ� �Լ�
        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
            base.OnShutdown(runner, shutdownReason);

            // �������� ���ᰡ �ƴ϶�� ���� �޼����� ����
            if (shutdownReason != ShutdownReason.Ok)
                PopupUI.Instance.OpenError(GetShutdownMessage(shutdownReason));
        }

        // ��Ʈ��ũ ���� ���� �޼����� ��ȯ�Ѵ�
        private static string GetShutdownMessage(ShutdownReason shutdownReason) {
            switch(shutdownReason) {
                case ShutdownReason.GameClosed:
                    return "����� ������ ������ϴ�!";

                case ShutdownReason.GameNotFound:
                    return "�ش� ���� �����ϴ�!";

                case ShutdownReason.GameIsFull:
                    return "�ο��� �� á���ϴ�!";

                case ShutdownReason.ConnectionRefused:
                    return "�̹� ������ �����߽��ϴ�!";

                default:
                    return "������ �߻��߽��ϴ�!";
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