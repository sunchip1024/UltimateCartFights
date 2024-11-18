using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltimateCartFights.Network;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateCartFights.UI {
    public class LoadingPanelUI : MonoBehaviour {

        #region Profile Section

        [Header("Profile Section")]
        [SerializeField] private GameObject ProfileGroup;
        [SerializeField] private PlayerUI ProfilePrefab;

        private List<PlayerUI> Profiles = new List<PlayerUI>();

        private void InitializeProfile() {
            // 기존에 있던 프로필 UI 제거
            foreach(PlayerUI profile in Profiles) {
                Destroy(profile.gameObject);
            }

            // 프로필 리스트 초기화
            Profiles.Clear();

            // ClientPlayer의 플레이어 정보들을 가져와 Player UI 생성
            for(int ID = 0; ID < RoomInfo.MAX_PLAYER; ID++) {
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.PlayerID == ID);
                if (client == null) continue;

                PlayerUI profile = Instantiate(ProfilePrefab, ProfileGroup.transform);
                profile.SetPlayerInfo((string) client.Nickname, client.Character, false);
                Profiles.Add(profile);
            }
        }

        #endregion

        #region Loading Section

        [Header("Loading Section")]
        [SerializeField] private Slider LoadingBar;
        [SerializeField] private TMP_Text LoadingPercent;
        [SerializeField] private TMP_Text Tip;

        [SerializeField] private List<string> TipList;

        [SerializeField] private float LOADING_SPEED;

        private const float DELTA_TIME = 0.01f;
        private const float TIP_CHANGE_COOLTIME = 5.0f;

        private static float targetProgress = 0.0f;
        private static float currentProgress = 0.0f;

        private static bool isStarted = false;

        private IEnumerator loadingCoroutine = null;
        private IEnumerator tipCoroutine = null;

        public void SetLoadingProgress(float progress) {
            targetProgress = progress;
        }

        public static bool IsLoadingComplete() {
            if (!isStarted) return false;
            if (currentProgress < 1.0f) return false;
            return true;
        }

        private void InitializeLoading() {
            targetProgress = 0.0f;
            currentProgress = 0.0f;
            isStarted = true;

            loadingCoroutine = ShowLoadingProcess();
            StartCoroutine(loadingCoroutine);

            tipCoroutine = ShowRandomTip();
            StartCoroutine(tipCoroutine);
        }

        private IEnumerator ShowLoadingProcess() {
            while (!IsLoadingComplete()) {
                yield return new WaitForSeconds(DELTA_TIME);

                // (LOADING_SPEED * DELTA_TIME) % 만큼 더한다
                currentProgress += (DELTA_TIME * LOADING_SPEED / 100.0f);
                // 목표 진행도 만큼만 설정
                currentProgress = Mathf.Min(currentProgress, targetProgress);

                // Loading UI에 정보 표시
                LoadingBar.value = currentProgress;
                LoadingPercent.text = string.Format("{0:0.#} %", currentProgress * 100.0f);
            }
        }

        private IEnumerator ShowRandomTip() {
            while(!IsLoadingComplete()) {
                int index = Random.Range(0, TipList.Count);
                Tip.text = TipList[index];

                yield return new WaitForSeconds(TIP_CHANGE_COOLTIME);
            }
        }

        #endregion

        #region Others

        public void Initialize() {
            InitializeProfile();
            InitializeLoading();
        }

        public void Disabled() {
            targetProgress = 0.0f;
            currentProgress = 0.0f;
            isStarted = false;
            
            StopCoroutine(loadingCoroutine);
            StopCoroutine(tipCoroutine);
        }

        #endregion
    }
}