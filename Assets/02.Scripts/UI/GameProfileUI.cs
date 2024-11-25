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
            // �ش� �÷��̾ �̹� ���ӿ������ ��û ����
            if (PlayerID == -1) return;
            // ���� ������ ���ӿ������� �ʾҴٸ� ��û ����
            if (CartController.Local != null) return;

            // �ش� �÷��̾� ID�� �ش��ϴ� īƮ�� ã�� ī�޶� Ÿ������ ����
            CartController cart = CartController.Carts.FirstOrDefault(x => x.PlayerID == PlayerID);
            CartCamera.SetTarget(cart);
        }
    }
}