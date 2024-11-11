using UnityEngine;
using UnityEngine.UI;
using UltimateCartFights.Utility;
using UltimateCartFights.Network;

namespace UltimateCartFights.UI {
    public class ColorSelectionUI : MonoBehaviour {

        [SerializeField] private Image Color;
        [SerializeField] private Image Block;

        private int ColorType;

        public void Initialize(int color) {
            ColorType = color;

            Color.color = ResourceManager.Instance.ColorMaterials[color].color;
            Color.gameObject.SetActive(true);
            Block.gameObject.SetActive(false);
        }

        public void SetBlock(bool isSelected) {
            Block.gameObject.SetActive(isSelected);
        } 

        public void OnChangeColor() {
            if (ClientPlayer.Local.CartColor == ColorType)
                ClientPlayer.Local.RPC_SetCartColor(-1);
            else
                ClientPlayer.Local.RPC_SetCartColor(ColorType);
        }
    }
}