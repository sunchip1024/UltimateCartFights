using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimateCartFights.UI;
using UltimateCartFights.Utility;
using UnityEngine;

namespace UltimateCartFights.Network {
    public class FusionSocket : NetworkBehaviour, INetworkRunnerCallbacks {

        #region Unity LifeCycle

        private static FusionSocket Instance = null;
        
        protected static NetworkRunner Runner { get; private set; } = null;

        protected static NetworkStateMachine stateManager = new NetworkStateMachine();

        // �κ� ���� ��ð� �ƴ϶� �κ� �� ����� ���� �� ���¸� LOADING_LOBBY���� LOBBY�� �ٲ۴�
        protected static bool isFirstSessionUpdate = true;

        private void Awake() {
            if (Instance != null) {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        #endregion

        #region Fusion Network Methods

        protected static async Task Open() {
            // ���� �̹� �������� ��ü�� �ִٸ� �ش� ��ü�� �����Ѵ�
            if (Runner != null)
                await Close();

            // �� ������ ���� NetworkRunner ��ü�� �����Ѵ�
            GameObject runnerObject = Instantiate(ResourceManager.Instance.Session);
            DontDestroyOnLoad(runnerObject);

            // NetworkRunner ������Ʈ�� �Ҵ��Ѵ�
            Runner = runnerObject.GetComponent<NetworkRunner>();
            Runner.ProvideInput = true;
            Runner.AddCallbacks(Instance);
        }

        protected static async Task JoinLobby() {
            StartGameResult result = await Runner.JoinSessionLobby(SessionLobby.ClientServer);

            if (!result.Ok)
                throw new Exception(result.ErrorMessage);
        }

        protected static async Task CreateRoom(RoomInfo room) {
            StartGameResult result = await Runner.StartGame(new StartGameArgs {
                GameMode = GameMode.Host,
                PlayerCount = room.MaxPlayer,
                SessionProperties = GetRoomProperties(room),
            });

            if(!result.Ok)
                throw new Exception(result.ErrorMessage);
        }

        protected static async Task JoinRoom(RoomInfo room) {
            StartGameResult result = await Runner.StartGame(new StartGameArgs {
                GameMode = GameMode.Client,
                SessionName = room.RoomID,
                PlayerCount = room.MaxPlayer,
                SessionProperties = GetRoomProperties(room),
                EnableClientSessionCreation = false,
            });

            if(!result.Ok)
                throw new Exception(result.ErrorMessage);
        }

        protected static async Task Close() {
            if (Runner == null)
                return;

            await Runner.Shutdown();
            Destroy(Runner.gameObject);
            Runner = null;

            isFirstSessionUpdate = true;
        }

        #endregion

        #region Other Method

        private static Dictionary<string, SessionProperty> GetRoomProperties(RoomInfo room) {
            Dictionary<string, SessionProperty> properties = new Dictionary<string, SessionProperty>();

            properties["RoomName"] = room.RoomName;
            properties["HostNickname"] = room.HostNickname;

            return properties;
        }

        #endregion

        #region INetworkRunnerCallbacks

        public virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public virtual void OnConnectedToServer(NetworkRunner runner) { }

        public virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public virtual void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public virtual void OnInput(NetworkRunner runner, NetworkInput input) { }

        public virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public virtual void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public virtual void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public virtual void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public virtual void OnSceneLoadDone(NetworkRunner runner) { }

        public virtual void OnSceneLoadStart(NetworkRunner runner) { }

        public virtual void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public virtual void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        #endregion
    }
}