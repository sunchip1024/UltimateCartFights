using System.Linq;
using System.Threading.Tasks;
using UltimateCartFights.Game;
using UltimateCartFights.UI;

namespace UltimateCartFights.Network {
    public class ResultState : INetworkState {

        public void Start() {
            CartController cart = CartController.Carts.FirstOrDefault(x => x.PlayerID == GameLauncher.WinnerID);
            ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.PlayerID == GameLauncher.WinnerID);

            PanelUI.Instance.SetPanel(PanelUI.Panel.RESULT);
            PanelUI.Instance.SetWinner((string) client.Nickname);

            CartCamera.SetTarget(cart);

            WaitAndReturn();
        }

        public void Update() { }

        public void Terminate() { }

        private async void WaitAndReturn() {
            // 5000ms (= 5s) 만큼 대기
            await Task.Delay(5000);

            // 다시 방으로 복귀한다
            GameLauncher.LoadRoom();
            GameLauncher.ReturnRoom();
        }
    }
}