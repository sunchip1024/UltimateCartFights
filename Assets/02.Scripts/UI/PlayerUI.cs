using TMPro;
using UltimateCartFights.Utility;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace UltimateCartFights.UI {
    public class PlayerUI : MonoBehaviour {

        [SerializeField] private TMP_Text NicknameText;
        [SerializeField] private GameObject NicknameBackground;
        [SerializeField] private GameObject ReadyImage;
        [SerializeField] private RawImage Character;
        [SerializeField] private GameObject BlockedBackground;

        public void SetPlayerInfo(string nickname, int character, bool isReady) {
            NicknameText.text = nickname;
            NicknameBackground.SetActive(!nickname.IsNullOrEmpty());

            if(character == -1)
                Character.gameObject.SetActive(false);
            else {
                Character.gameObject.SetActive(true);
                Character.texture = ConvertSpriteToTexture(ResourceManager.Instance.Characters[character]);
            }
            
            ReadyImage.SetActive(isReady);
            BlockedBackground.SetActive(false);
        }

        public void SetEmpty() {
            SetPlayerInfo(string.Empty, -1, false);
        }

        public void SetBlocked() {
            SetPlayerInfo(string.Empty, -1, false);
            BlockedBackground.SetActive(true);
        }


        private Texture2D ConvertSpriteToTexture(Sprite sprite) {
            if(sprite.rect.width == sprite.texture.width)
                return sprite.texture;

            Texture2D texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            Color[] colors = sprite.texture.GetPixels((int) sprite.textureRect.x,
                                                      (int) sprite.textureRect.y, 
                                                      (int) sprite.textureRect.width,
                                                      (int) sprite.textureRect.height);

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }
    }
}