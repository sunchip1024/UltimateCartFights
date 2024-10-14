using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class LobbyState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetLobby();
        }

        public void Update() { }

        public void Terminate() { }
    }
}