using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class RoomState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.ROOM);
            PanelUI.Instance.InitializeRoom();
            
            ChatNetwork.Open(GameLauncher.GetSessionInfo().Name);
        }

        public void Update() { }

        public void Terminate() { 
            PanelUI.Instance.LeaveRoom();
            
            ChatNetwork.Close();
        }
    }
}