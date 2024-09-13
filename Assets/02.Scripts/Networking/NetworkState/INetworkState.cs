namespace UltimateCartFights.Network {

    public enum STATE { 
        NONE, 
        CLOSED, 
        LOADING_LOBBY, 
        LOBBY, 
        ROOM, 
        LOADING_GAME,
        GAME,
        RESULT
    };

    public interface INetworkState {
        public void Start();
        public void Update();
        public void Terminate();
    }
}