using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FusionTest : MonoBehaviour, INetworkRunnerCallbacks
{

    private NetworkRunner runner = null;

    private void Awake() {
        Open();
    }

    private void OnDestroy() {
        Close();
    }

    private async void Open() {
        if(runner != null)
            await Close();

        GameObject runnerObject = new GameObject("Session");
        DontDestroyOnLoad(runnerObject);

        runner = runnerObject.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);
        Debug.Log($"Create GameObject {runnerObject.name} - Starting Game");

        StartGameResult result = await runner.StartGame(new StartGameArgs {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "TestRoom",
            PlayerCount = 6
        });

        Debug.Log($"Connnection to Rooom - Result : {result.Ok} ");

        if(!result.Ok)
            await Close();
    }

    private async Task Close() {
        if (runner == null) return;

        await runner.Shutdown();
        Destroy(runner.gameObject);
        runner = null;
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        Debug.Log($"Player {player.PlayerId} Joined!");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        Debug.Log($"Player {player.PlayerId} Left!");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
