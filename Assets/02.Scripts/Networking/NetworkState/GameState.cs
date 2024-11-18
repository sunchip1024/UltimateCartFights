using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class GameState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.GAME);
        }

        public void Update() { }

        public void Terminate() { }
    }
}