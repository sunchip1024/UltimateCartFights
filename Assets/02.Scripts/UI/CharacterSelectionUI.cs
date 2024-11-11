using UnityEngine;
using UnityEngine.UI;
using UltimateCartFights.Utility;
using UltimateCartFights.Network;

namespace UltimateCartFights.UI {
    public class CharacterSelectionUI : MonoBehaviour {

        [SerializeField] private Image Character;
        [SerializeField] private Image Blocked;

        private int CharacterType;

        public void Initialize(int type) {
            CharacterType = type;

            Character.sprite = ResourceManager.Instance.Characters[type];
            Character.gameObject.SetActive(true);
            Blocked.gameObject.SetActive(false);
        }

        public void SetBlock(bool isSelected) {
            Blocked.gameObject.SetActive(isSelected);
        }

        public void OnChangeCharacter() {
            // �̹� �ڽ��� �� ĳ���͸� �ٽ� ������ ��� - ĳ���� ���
            if (ClientPlayer.Local.Character == CharacterType)
                ClientPlayer.Local.RPC_SetCharacter(-1);
            else
                ClientPlayer.Local.RPC_SetCharacter(CharacterType);
        }
    }
}