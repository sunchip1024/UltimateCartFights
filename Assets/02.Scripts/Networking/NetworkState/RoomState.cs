using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class RoomState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.ROOM);
        }

        public void Update() { }

        public void Terminate() { }
    }
}