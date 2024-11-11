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
            // 1. 0번부터 5번 사이에서 빈 PlayerID를 찾는다
            for(int playerID = 0; playerID < RoomInfo.MAX_PLAYER; playerID++) {
                if (IsUsedPlayerID(playerID)) continue;

                this.PlayerID = playerID;
                break;
            }

            // 2. 마찬가지로 사용하지 않는 캐릭터 번호를 찾는다
            for (int character = 0; character < RoomInfo.MAX_PLAYER; character++) {
                if (IsUsedCharacter(character)) continue;

                this.Character = character;
                break;
            }

            // 3. 마찬가지로 사용하지 않는 색 번호를 찾는다
            for (int color = 0; color < RoomInfo.MAX_PLAYER; color++) {
                if (IsUsedCartColor(color)) continue;

                this.CartColor = color;
                break;
            }

            // 4. 닉네임을 저장한다
            this.Nickname = nickname;

            // 방장은 자동으로 준비 상태를 체크하여 변경한다
            if (PlayerID == 0)
                IsReady = CanReady();
            // 다른 클라이언트들은 준비 상태를 해제한다
            else
                IsReady = false;
        }

        [Rpc(sources:RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetCharacter(int character) {
            if(IsUsedCharacter(character)) return;
            this.Character = character;

            // 방장은 자동으로 준비 상태를 체크하여 변경한다
            if (PlayerID == 0)
                IsReady = CanReady();
            // 다른 클라이언트들은 준비 상태를 해제한다
            else
                IsReady = false;

        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetCartColor(int color) {
            if (IsUsedCartColor(color)) return;
            this.CartColor = color;

            // 방장은 자동으로 준비 상태를 체크하여 변경한다
            if (PlayerID == 0)
                IsReady = CanReady();
            // 다른 클라이언트들은 준비 상태를 해제한다
            else
                IsReady = false;
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetReady(NetworkBool isReady) {
            // 준비 상태가 불가능한 경우에는 준비 상태로 변경하지 않는다
            if (!CanReady() && isReady) return;

            this.IsReady = isReady;
        }

        #endregion

        #region Others

        public static ClientPlayer RemovePlayer(PlayerRef player) {
            // 해당 클라이언트의 캐릭터가 존재하는지 검색
            ClientPlayer client = Players.FirstOrDefault(x => x.Object.InputAuthority == player);

            // 만일 존재한다면 해당 클라이언트의 캐릭터를 목록에서 제거
            if (client != null) 
                Players.Remove(client);

            // 제거한 캐릭터를 반환
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