using System.Linq;
using TMPro;
using UltimateCartFights.Game;
using UltimateCartFights.Network;
using UltimateCartFights.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateCartFights.UI {
    public class GameProfileUI : MonoBehaviour {
        [Header("Profile UI")]
        [SerializeField] private Image characterImage;
        [SerializeField] private Image knockoutImage;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text damageText;

        [Header("Damage Font Size")]
        [SerializeField] private float INTEGER_FONT_SIZE;
        [SerializeField] private float DECIMAL_FONT_SIZE;

        private int PlayerID = -1;

        public void Initialize(ClientPlayer client) {
            characterImage.sprite = ResourceManager.Instance.Characters[client.Character];
            knockoutImage.gameObject.SetActive(false);

            nicknameText.text = (string) client.Nickname;
            SetDamage(0.0f);

            PlayerID = client.PlayerID;
        }

        public void SetDamage(float damage) {
            damage = Mathf.Max(damage, CartController.DAMAGE_LIMIT);

            int integerNum = (int) damage;
            int decimalNum = ((int) damage * 10) % 10;

            string color;
            const float DAMAGE_PART_LIMIT = CartController.DAMAGE_LIMIT / 3f;
            if (damage < DAMAGE_PART_LIMIT)             color = "ffe404";
            else if (damage < DAMAGE_PART_LIMIT * 2f)   color = "f77618";
            else                                        color = "f71818";

            damageText.text = string.Format("<#{0}><size={1}>{2}</size><size={3}>.{4}%</size></color>",
                color, INTEGER_FONT_SIZE, integerNum, DECIMAL_FONT_SIZE, decimalNum);
        }

        public void Knockout() {
            knockoutImage.gameObject.SetActive(true);
            PlayerID = -1;
        }

        public void OnClickObserving() {
            // 해당 플레이어가 이미 게임오버라면 요청 무시
            if (PlayerID == -1) return;
            // 아직 본인이 게임오버되지 않았다면 요청 무시
            if (CartController.Local != null) return;

            // 해당 플레이어 ID에 해당하는 카트를 찾아 카메라 타겟으로 설정
            CartController cart = CartController.Carts.FirstOrDefault(x => x.PlayerID == PlayerID);
            CartCamera.SetTarget(cart);
        }
    }
}