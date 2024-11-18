using AYellowpaper.SerializedCollections;
using Fusion;
using System.Collections;
using System.Linq;
using UltimateCartFights.Game;
using UltimateCartFights.UI;
using UltimateCartFights.Utility;
using UnityEngine;

namespace UltimateCartFights.Network {

    public enum SCENE { LOBBY, ROOM, GAME };

    public class SceneManager : NetworkSceneManagerDefault {

        #region Scene Loading - Public Method

        [SerializedDictionary("Scene Type", "Scene Index Number")]
        [SerializeField] private SerializedDictionary<SCENE, int> Scenes;

        public void LoadScene(SCENE scene) {
            switch(scene) {
                case SCENE.LOBBY:
                    StartCoroutine(LoadLobbyScene());
                    break;

                case SCENE.ROOM:
                case SCENE.GAME:
                    Runner.LoadScene(SceneRef.FromIndex(Scenes[scene]));
                    break;
            }
        }

        #endregion

        #region Scene Loading - Spawn Method

        private void SpawnCarts() {
            foreach(ClientPlayer client in ClientPlayer.Players) {
                // īƮ ���� �� �ʿ��� ���� �ҷ�����
                Transform spawn = Map.Current.SpawnPoints[client.PlayerID];
                PlayerRef input = client.Object.InputAuthority;

                // īƮ ����
                NetworkObject cart = Runner.Spawn(
                    ResourceManager.Instance.Cart,
                    spawn.position,
                    spawn.rotation,
                    input
                );

                // īƮ �̸� ����
                cart.transform.name = string.Format("Cart_{0}_{1}", client.PlayerID, client.Nickname);
            }
        }

        private bool IsSpawnCompleted() {
            return ClientPlayer.Players.Count == CartController.Carts.Count;
        }

        #endregion

        #region Scene Loading - Override Method

        protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams) {
            // ���� ������ �ε��Ѵٸ� ���� �ε� ���·� �����Ѵ�
            if (IsGameScene(sceneRef))
                GameLauncher.StartLoading();
            // ���ӷ����� �̵��Ѵٸ� Fade UI�� ǥ���Ѵ�
            else
                PanelUI.Instance.SetPanel(PanelUI.Panel.FADE);
            
            // �� �ε��� �Ϸ�� ������ ����Ѵ� (�� �ε� �Ϸ� �� �� ������ �� ����Ѵ�)
            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
            yield return null;

            if(IsGameScene(sceneRef)) {
                // ȣ��Ʈ��� īƮ���� ����
                if(GameLauncher.IsHost())
                    SpawnCarts();

                // ��� īƮ���� ������ ������ ���
                yield return new WaitUntil(IsSpawnCompleted);

                // ��ǥ ���൵�� 100%�� ���� �� �ε� �ٰ� �� �������� ���
                PanelUI.Instance.SetLoadingProgress(1.0f);
                yield return new WaitUntil(() => LoadingPanelUI.IsLoadingComplete());
            }

            // ���� �� �ε��� �Ϸ�Ǿ��ٸ� ���� ���·� �����Ѵ�
            if(IsGameScene(sceneRef))
                GameLauncher.StartGame();
            // ���ӷ����� �̵��� ��� ���ӷ� ���·� �����Ѵ�
            else
                GameLauncher.ReturnRoom();
        }

        protected IEnumerator LoadLobbyScene() {
            // �̹� �κ� ���̶�� �ε��� ���� �ʴ´�
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.buildIndex == Scenes[SCENE.LOBBY]) yield break;

            // Fade UI�� ǥ���Ѵ�
            PanelUI.Instance.SetPanel(PanelUI.Panel.FADE);

            // �񵿱������� �� �ε��� �����Ѵ�
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Scenes[SCENE.LOBBY]);

            // �� �ε��� �Ϸ�� ������ ��ٸ���
            while(!op.isDone) {
                yield return null;
            }

            // �� �ε��� �Ϸ�� �� �� ������ �� ����Ѵ�
            yield return null;
        }

        protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress) {
            base.OnLoadSceneProgress(sceneRef, progress);

            // ���� �ε� ���̶�� ���� �ε� ������ UI�� �����Ѵ�
            if (IsGameScene(sceneRef))
                PanelUI.Instance.SetLoadingProgress(progress);
        }

        #endregion

        #region Others

        private bool IsGameScene(SceneRef sceneRef) {
            SCENE scene = Scenes.FirstOrDefault(x => x.Value == sceneRef.AsIndex).Key;

            switch(scene) {
                case SCENE.LOBBY:
                case SCENE.ROOM:
                    return false;

                case SCENE.GAME:
                    return true;
            }

            throw new System.Exception("Unknowned scene reference");
        }

        #endregion
    }
}