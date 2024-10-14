using UnityEngine;

namespace UltimateCartFights.Utility {
    public static class ClientInfo {
        public static string Nickname {
            get => PlayerPrefs.GetString("C_Nickname", string.Empty);
            set => PlayerPrefs.SetString("C_Nickname", value.Trim());
        }
    }
}