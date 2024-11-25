using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UltimateCartFights.Game {
    public class CartInput : NetworkBehaviour, INetworkRunnerCallbacks {

        #region Cart Input Method

        [SerializeField] private InputAction accelerataion;
        [SerializeField] private InputAction steer;
        [SerializeField] private InputAction dash;

        private Gamepad gamepad;
        private bool wasDashed;

        public void OnInput(NetworkRunner runner, NetworkInput input) {
            // ���� ����� ��Ʈ�ѷ� ������ ������
            gamepad = Gamepad.current;

            InputData current = new InputData();

            // ����, ȸ�� �Է� ���� ����
            current.Acceleration = accelerataion.ReadValue<float>();
            current.Steer = steer.ReadValue<float>();

            // �뽬Ű �Է� ���� ����
            current.IsDash = dash.IsPressed();
            current.IsDashDown = (!wasDashed) && (current.IsDash);

            wasDashed = current.IsDash;

            input.Set(current);
        }

        #endregion

        #region Unity LifeCycle Method

        public override void Spawned() {
            base.Spawned();
            Runner.AddCallbacks(this);

            accelerataion = accelerataion.Clone();
            steer = steer.Clone();
            dash = dash.Clone();

            accelerataion.Enable();
            steer.Enable();
            dash.Enable();
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            base.Despawned(runner, hasState);
            Runner.RemoveCallbacks(this);

            accelerataion.Disable();
            steer.Disable();
            dash.Disable();
        }

        #endregion

        #region INetworkRunnerCallbacks Method

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        #endregion
    }

    public struct InputData : INetworkInput{

        // ���� (��/����)
        private int _acceleration;
        public float Acceleration {
            get { return _acceleration * 0.001f; }
            set { _acceleration = (int) (value * 1000); }
        }

        // ȸ�� (��/��ȸ��)
        private int _steer;
        public float Steer {
            get { return _steer * 0.001f; }
            set { _steer = (int) (value * 1000); }
        }

        // �뽬
        public bool IsDash;        // �뽬Ű�� ������ �ִ��� Ȯ��
        public bool IsDashDown;    // ���� �����ӿ� �뽬Ű�� ���������� Ȯ��
    }
}