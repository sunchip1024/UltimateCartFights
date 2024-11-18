using Fusion;
using System.Collections.Generic;
using UltimateCartFights.Network;
using UltimateCartFights.Utility;
using UnityEngine;

namespace UltimateCartFights.Game {
    public class Map : NetworkBehaviour {

        #region Map Information

        // 현재 게임을 진행중인 맵
        public static Map Current { get; private set; } = null;

        // 스폰포인트 위치 정보
        public List<Transform> SpawnPoints;

        // 게임 시작 카운트 타이머
        [Networked] public TickTimer StartTimer { get; private set; }

        // 게임 로딩 후 대기시간
        private const float INTRO_DURATION = 9.0f;

        #endregion

        #region Unity LifeCycle Method

        private void Awake() {
            Current = this;
        }

        private void OnDestroy() {
            Current = null;
        }

        public override void Spawned() {
            base.Spawned();

            if (GameLauncher.IsHost())
                StartTimer = TickTimer.CreateFromSeconds(Runner, INTRO_DURATION);
        }

        #endregion

        #region Others

        public static bool IsGameStarted() {
            if (Current == null) return false;
            return Current.StartTimer.Expired(Current.Runner);
        }

        public static float GetRemainingTime() {
            if (Current == null) return Mathf.Infinity;
            if (!Current.StartTimer.IsRunning) return Mathf.Infinity;
            return (float) Current.StartTimer.RemainingTime(Current.Runner);
        }

        #endregion
    }
}