using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class GameState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.GAME);
            PanelUI.Instance.InitializeGame();

            // 만일 중간에 플레이어가 나가 한 명만 남았는지 확인
            GameLauncher.CheckGameEnded();
        }

        public void Update() { 
            PanelUI.Instance.UpdateGame();
        }

        public void Terminate() { }
    }
}