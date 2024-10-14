using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class CloseState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.INTRO);
        }

        public void Update() { }

        public void Terminate() { }
    }
}