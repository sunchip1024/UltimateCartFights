using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class GameState : INetworkState {

        public void Start() {
            PanelUI.Instance.SetPanel(PanelUI.Panel.GAME);
            PanelUI.Instance.InitializeGame();

            // ���� �߰��� �÷��̾ ���� �� �� ���Ҵ��� Ȯ��
            GameLauncher.CheckGameEnded();
        }

        public void Update() { 
            PanelUI.Instance.UpdateGame();
        }

        public void Terminate() { }
    }
}