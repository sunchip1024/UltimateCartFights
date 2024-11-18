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
                // 카트 생성 시 필요한 정보 불러오기
                Transform spawn = Map.Current.SpawnPoints[client.PlayerID];
                PlayerRef input = client.Object.InputAuthority;

                // 카트 생성
                NetworkObject cart = Runner.Spawn(
                    ResourceManager.Instance.Cart,
                    spawn.position,
                    spawn.rotation,
                    input
                );

                // 카트 이름 변경
                cart.transform.name = string.Format("Cart_{0}_{1}", client.PlayerID, client.Nickname);
            }
        }

        private bool IsSpawnCompleted() {
            return ClientPlayer.Players.Count == CartController.Carts.Count;
        }

        #endregion

        #region Scene Loading - Override Method

        protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams) {
            // 게임 씬으로 로딩한다면 게임 로딩 상태로 변경한다
            if (IsGameScene(sceneRef))
                GameLauncher.StartLoading();
            // 게임룸으로 이동한다면 Fade UI를 표시한다
            else
                PanelUI.Instance.SetPanel(PanelUI.Panel.FADE);
            
            // 씬 로딩이 완료될 때까지 대기한다 (씬 로딩 완료 후 한 프레임 더 대기한다)
            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
            yield return null;

            if(IsGameScene(sceneRef)) {
                // 호스트라면 카트들을 생성
                if(GameLauncher.IsHost())
                    SpawnCarts();

                // 모든 카트들이 생성될 때까지 대기
                yield return new WaitUntil(IsSpawnCompleted);

                // 목표 진행도를 100%로 설정 후 로딩 바가 다 찰때까지 대기
                PanelUI.Instance.SetLoadingProgress(1.0f);
                yield return new WaitUntil(() => LoadingPanelUI.IsLoadingComplete());
            }

            // 게임 씬 로딩이 완료되었다면 게임 상태로 변경한다
            if(IsGameScene(sceneRef))
                GameLauncher.StartGame();
            // 게임룸으로 이동한 경우 게임룸 상태로 변경한다
            else
                GameLauncher.ReturnRoom();
        }

        protected IEnumerator LoadLobbyScene() {
            // 이미 로비 씬이라면 로딩을 하지 않는다
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.buildIndex == Scenes[SCENE.LOBBY]) yield break;

            // Fade UI를 표시한다
            PanelUI.Instance.SetPanel(PanelUI.Panel.FADE);

            // 비동기적으로 씬 로딩을 진행한다
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Scenes[SCENE.LOBBY]);

            // 씬 로딩이 완료될 때까지 기다린다
            while(!op.isDone) {
                yield return null;
            }

            // 씬 로딩이 완료된 후 한 프레임 더 대기한다
            yield return null;
        }

        protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress) {
            base.OnLoadSceneProgress(sceneRef, progress);

            // 게임 로딩 중이라면 현재 로딩 정도를 UI에 전달한다
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