using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class LoadingLobbyState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.LOBBY);
            PanelUI.Instance.SetLoading();
            GameLauncher.JoinLobby();
        }

        public void Update() { }

        public void Terminate() { }
    }
}
