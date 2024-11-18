using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class LoadingGameState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.LOADING);
            PanelUI.Instance.InitializeLoading();
        }

        public void Update() { }

        public void Terminate() { 
            PanelUI.Instance.DisableLoading();
        }
    }
}