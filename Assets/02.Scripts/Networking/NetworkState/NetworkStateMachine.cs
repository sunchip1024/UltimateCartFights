using System.Collections.Generic;

namespace UltimateCartFights.Network {
    public class NetworkStateMachine {

        private const STATE DEFAULT_STATE = STATE.CLOSED;

        private static readonly Dictionary<STATE, INetworkState> States = new() {
            { STATE.NONE, new NoneState() },
            { STATE.CLOSED, new CloseState() },
            { STATE.LOADING_LOBBY, new LoadingLobbyState() },
            { STATE.LOBBY, new LobbyState() },
            { STATE.ROOM, new RoomState() },
            { STATE.LOADING_GAME, new LoadingGameState() },
            { STATE.GAME, new GameState() },
            { STATE.RESULT, new ResultState() },
        };

        private INetworkState current = States[DEFAULT_STATE];
        public STATE State { get; private set; } = DEFAULT_STATE;

        private void SetState(STATE state) {
            this.State = state;
            this.current = States[state];
        }

        public void Start() {
            current.Start();
        }

        public void Update() {
            current.Update();
        }

        public void StopState() {
            current.Terminate();
            SetState(STATE.NONE);
        }

        public void StartState(STATE state) {
            SetState(state);
            current.Start();
        }

        public void Abort() {
            StopState();
            StartState(DEFAULT_STATE);
        }
    }
}