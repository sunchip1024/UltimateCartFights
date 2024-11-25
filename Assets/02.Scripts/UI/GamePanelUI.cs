using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltimateCartFights.Game;
using UltimateCartFights.Network;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateCartFights.UI {
    public class GamePanelUI : MonoBehaviour {

        #region Profile Section

        [Header("Profile Section")]
        [SerializeField] private GameProfileUI mainProfile;
        [SerializeField] private GameProfileUI profilePrefab;
        [SerializeField] private Transform profileGroup;

        private List<GameProfileUI> profiles = new List<GameProfileUI>();

        private void InitializeProfile() {
            // ������ �ִ� ���� ������ UI ��� ����
            GameProfileUI[] befores = profileGroup.GetComponentsInChildren<GameProfileUI>();
            foreach (GameProfileUI profile in befores) {
                Destroy(profile.gameObject);
            }

            profiles.Clear();

            // ���� �濡 �ִ� �÷��̾� �������� �� ������ ����
            for(int i = 0; i < RoomInfo.MAX_PLAYER; i++) {
                ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => x.PlayerID == i);
                GameProfileUI profile = null;

                if(client != null) {
                    if(ClientPlayer.Local.PlayerID == i)
                        profile = mainProfile;
                    else
                        profile = Instantiate(profilePrefab, profileGroup);
                    
                    profile.Initialize(client);
                }

                profiles.Add(profile);
            }

        }

        public void OnGetDamaged(int playerID, float damage) {
            if (profiles[playerID] == null) return;
            profiles[playerID].SetDamage(damage);
        }

        public void OnKnockout(int playerID) {
            if (profiles[playerID] == null) return;
            profiles[playerID].Knockout();
        }

        #endregion

        #region Game State Section

        [Header("Game State Section")]
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text dashCoolText;
        [SerializeField] private Image dashCoolIcon;

        private void UpdateGameState() {
            // �ڽ��� �÷��̾ ���ӿ������ �������� ����
            if (CartController.Local == null) return;

            // ���� �÷��̾��� �ӵ� ǥ��
            float speed = Mathf.Abs(CartController.Local.appliedSpeed);
            speedText.text = string.Format("{0:f2}", speed);

            // ��� ��ų�� ��Ÿ�� ǥ��
            float remain = CartController.Local.GetDashCooltime();
            float total = CartController.DASH_COOLTIME;

            dashCoolText.gameObject.SetActive(remain > 0);
            dashCoolText.text = string.Format("{0:f1}s", remain);
            dashCoolIcon.fillAmount = remain / total;
        }

        #endregion

        #region Countdown Section

        [Header("Countdown Section")]
        [SerializeField] private Animation countAnim;

        private bool hasCountStarted = false;

        private void UpdateCount() {
            if (hasCountStarted) return;

            if(Map.GetRemainingTime() <= 3f) { 
                hasCountStarted = true;
                countAnim.Play();
            }
        }

        #endregion

        #region Others

        public void InitializeGameUI() {
            InitializeProfile();
            hasCountStarted = false;
        }

        public void UpdateGameUI() {
            UpdateGameState();
            UpdateCount();
        }

        #endregion
    }
}