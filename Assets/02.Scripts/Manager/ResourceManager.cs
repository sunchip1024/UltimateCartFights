using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateCartFights.Utility {
    public class ResourceManager : MonoBehaviour {

        [Header("Network Objects")]
        public GameObject Session;
        public GameObject Client;
        public GameObject Cart;

        [Header("Character Sprites")]
        public List<Sprite> Characters;

        [Header("Cart Color Material")]
        public List<Material> ColorMaterials;

        public static ResourceManager Instance = null;

        private void Awake() {
            if(Instance) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}