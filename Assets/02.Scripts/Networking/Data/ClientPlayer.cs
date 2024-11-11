using Fusion;
using System.Collections.Generic;
using System.Linq;
using UltimateCartFights.Utility;
using UnityEngine.Events;

namespace UltimateCartFights.Network {
    public class ClientPlayer : NetworkBehaviour {
        
        #region Player State

        public readonly static List<ClientPlayer> Players = new List<ClientPlayer>();

        public static ClientPlayer Local { get; private set; } = null;

        [Networked] public int PlayerID { get; private set; } = -1;
        [Networked] public int Character { get; private set; } = -1;
        [Networked] public int CartColor { get; private set; } = -1;
        [Networked] public NetworkBool IsReady { get; private set; } = false;
        [Networked] public NetworkString<_32> Nickname { get; private set; } = string.Empty;

        #endregion

        #region Player Lifecycle Method

        private ChangeDetector changeDetector;

        public readonly static UnityEvent PlayerUpdated = new UnityEvent();

        public override void Spawned() {
            base.Spawned();

            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if(Object.HasInputAuthority) {
                Local = this;
                RPC_SetPlayerState(ClientInfo.Nickname);
            }

            Players.Add(this);

            DontDestroyOnLoad(gameObject);
        }

        public override void Render() {
            base.Render();

            foreach(string change in changeDetector.DetectChanges(this)) {
                switch(change) {
                    case nameof(PlayerID):
                    case nameof(Character):
                    case nameof(CartColor):
                    case nameof(IsReady):
                        PlayerUpdated.Invoke();
                        break;
                }
            }
        }

        private void OnDisable() {
            if (Local == this)
                Local = null;

            Players.Remove(this);
            PlayerUpdated.Invoke();
        }

        #endregion

        #region Client RPC Method

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetPlayerState(NetworkString<_32> nickname) { 
            // 1. 0������ 5�� ���̿��� �� PlayerID�� ã�´�
            for(int playerID = 0; playerID < RoomInfo.MAX_PLAYER; playerID++) {
                if (IsUsedPlayerID(playerID)) continue;

                this.PlayerID = playerID;
                break;
            }

            // 2. ���������� ������� �ʴ� ĳ���� ��ȣ�� ã�´�
            for (int character = 0; character < RoomInfo.MAX_PLAYER; character++) {
                if (IsUsedCharacter(character)) continue;

                this.Character = character;
                break;
            }

            // 3. ���������� ������� �ʴ� �� ��ȣ�� ã�´�
            for (int color = 0; color < RoomInfo.MAX_PLAYER; color++) {
                if (IsUsedCartColor(color)) continue;

                this.CartColor = color;
                break;
            }

            // 4. �г����� �����Ѵ�
            this.Nickname = nickname;

            // ������ �ڵ����� �غ� ���¸� üũ�Ͽ� �����Ѵ�
            if (PlayerID == 0)
                IsReady = CanReady();
            // �ٸ� Ŭ���̾�Ʈ���� �غ� ���¸� �����Ѵ�
            else
                IsReady = false;
        }

        [Rpc(sources:RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetCharacter(int character) {
            if(IsUsedCharacter(character)) return;
            this.Character = character;

            // ������ �ڵ����� �غ� ���¸� üũ�Ͽ� �����Ѵ�
            if (PlayerID == 0)
                IsReady = CanReady();
            // �ٸ� Ŭ���̾�Ʈ���� �غ� ���¸� �����Ѵ�
            else
                IsReady = false;

        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetCartColor(int color) {
            if (IsUsedCartColor(color)) return;
            this.CartColor = color;

            // ������ �ڵ����� �غ� ���¸� üũ�Ͽ� �����Ѵ�
            if (PlayerID == 0)
                IsReady = CanReady();
            // �ٸ� Ŭ���̾�Ʈ���� �غ� ���¸� �����Ѵ�
            else
                IsReady = false;
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetReady(NetworkBool isReady) {
            // �غ� ���°� �Ұ����� ��쿡�� �غ� ���·� �������� �ʴ´�
            if (!CanReady() && isReady) return;

            this.IsReady = isReady;
        }

        #endregion

        #region Others

        public static ClientPlayer RemovePlayer(PlayerRef player) {
            // �ش� Ŭ���̾�Ʈ�� ĳ���Ͱ� �����ϴ��� �˻�
            ClientPlayer client = Players.FirstOrDefault(x => x.Object.InputAuthority == player);

            // ���� �����Ѵٸ� �ش� Ŭ���̾�Ʈ�� ĳ���͸� ��Ͽ��� ����
            if (client != null) 
                Players.Remove(client);

            // ������ ĳ���͸� ��ȯ
            return client;
        }

        private bool IsUsedPlayerID(int playerID) {
            foreach(ClientPlayer player in Players) {
                if (player.PlayerID == playerID)
                    return true;
            }

            return false;
        }

        private bool IsUsedCharacter(int character) {
            foreach(ClientPlayer player in Players) {
                if(player.Character == character) 
                    return true;
            }

            return false;
        }

        private bool IsUsedCartColor(int color) {
            foreach(ClientPlayer player in Players) {
                if(player.CartColor == color) 
                    return true;
            }

            return false;
        }

        public static bool CanReady() {
            if (Local == null) return false;
            if (Local.Character == -1) return false;
            if (Local.CartColor == -1) return false;
            return true;
        }

        public static bool CanStartGame() {
            foreach(ClientPlayer player in Players) {
                if (!player.IsReady) 
                    return false;
            }

            return true;
        }

        #endregion
    }
}