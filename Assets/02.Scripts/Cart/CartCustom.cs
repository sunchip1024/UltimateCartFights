using UltimateCartFights.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateCartFights.Game {
    public class CartCustom : MonoBehaviour {

        [SerializeField] private SkinnedMeshRenderer renderer;
        [SerializeField] private Image backCharacter;
        [SerializeField] private Image frontCharacter;

        public void SetCharacter(int character) {
            backCharacter.sprite = ResourceManager.Instance.Characters[character];
            frontCharacter.sprite = ResourceManager.Instance.Characters[character];
        }

        public void SetColor(int color) {
            renderer.material = ResourceManager.Instance.ColorMaterials[color];
        }
    }
}