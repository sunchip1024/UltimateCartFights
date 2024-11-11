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
            // 이미 자신이 고른 캐릭터를 다시 선택한 경우 - 캐릭터 취소
            if (ClientPlayer.Local.Character == CharacterType)
                ClientPlayer.Local.RPC_SetCharacter(-1);
            else
                ClientPlayer.Local.RPC_SetCharacter(CharacterType);
        }
    }
}