using ExitGames.Client.Photon;
using Fusion.Photon.Realtime;
using Photon.Chat;
using UltimateCartFights.Utility;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

namespace UltimateCartFights.Network {

    public enum ChatType { SYSTEM, GENERAL, NONE };

    public class ChatNetwork : MonoBehaviour, IChatClientListener {

        #region Unity Lifecycle Method

        private static ChatNetwork Instance = null;

        private void Start() {
            Instance = this;
        }

        private void Update() {
            // chatClient가 존재하는 동안 Update에서 항상 Service 함수를 호출해야 한다
            if (chatClient != null)
                chatClient.Service();
        }

        private void OnDestroy() {
            Close();
        }

        private void OnApplicationQuit() {
            Close();
        }

        #endregion

        #region Chat Connection Method

        private static ChatClient chatClient = null;
        private static string chatChannel = string.Empty;

        public static void Open(string channel) {
            FusionAppSettings photonSetting = PhotonAppSettings.Global.AppSettings;
            Photon.Chat.AuthenticationValues authValue = new Photon.Chat.AuthenticationValues();

            chatChannel = channel;

            chatClient = new ChatClient(Instance);
            chatClient.Connect(photonSetting.AppIdChat, photonSetting.AppVersion, authValue);
        }

        public static void Close() {
            if (chatClient == null) return;

            chatClient.Disconnect();

            chatClient = null;
            chatChannel = string.Empty;
        }

        #endregion

        #region Chat Transmission Method

        private const string SYSTEM_TAG = "<SYSTEM>";
        private const string GENERAL_TAG = "<GENERAL>";

        public readonly static UnityEvent<string, ChatType> GetMessage = new UnityEvent<string, ChatType>();

        public static void SendChat(string message, ChatType type) {
            if (chatClient == null) return;
            if (chatChannel.IsNullOrEmpty()) return;
            if (message.IsNullOrEmpty()) return;

            string packet = AssembleChat(ClientInfo.Nickname, message, type);

            bool result = chatClient.PublishMessage(chatChannel, packet);

            if(!result)
                GetMessage.Invoke("[ SYSTEM : 채팅을 이용할 수 없습니다. 잠시 후에 시도해보세요. ]", ChatType.SYSTEM);
        }

        private static string AssembleChat(string sender, string message, ChatType type) {
            switch(type) {
                case ChatType.SYSTEM:
                    return string.Format("{0} [ SYSTEM : {1} ]", SYSTEM_TAG, message);

                case ChatType.GENERAL:
                    return string.Format("{0} {1} : {2}", GENERAL_TAG, sender, message);

                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Chat Event Callbacks

        public void OnConnected() {
            chatClient.Subscribe(new string[] { chatChannel });
        }

        public void OnSubscribed(string[] channels, bool[] results) {
            SendChat(string.Format("{0} 님이 입장했습니다!", ClientInfo.Nickname), ChatType.SYSTEM);
        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages) { 
            foreach(string packet in messages) {
                string[] tokens = packet.Split(new char[] { ' ' }, 2);

                ChatType type = ChatType.NONE;
                if (tokens[0].Equals(SYSTEM_TAG))       type = ChatType.SYSTEM;
                else if (tokens[0].Equals(GENERAL_TAG)) type = ChatType.GENERAL;

                GetMessage.Invoke(tokens[1], type);
            }
        }

        #endregion

        #region Other IChatClientLiestner Callbacks

        public void DebugReturn(DebugLevel level, string message) { }

        public void OnChatStateChange(ChatState state) { }

        public void OnDisconnected() { }

        public void OnPrivateMessage(string sender, object message, string channelName) { }

        public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }

        public void OnUnsubscribed(string[] channels) { }

        public void OnUserSubscribed(string channel, string user) { }

        public void OnUserUnsubscribed(string channel, string user) { }

        #endregion
    }
}