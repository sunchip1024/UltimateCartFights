using Fusion;
using System.Collections.Generic;
using UltimateCartFights.Network;
using UltimateCartFights.Utility;
using UnityEngine;

namespace UltimateCartFights.Game {
    public class Map : NetworkBehaviour {

        #region Map Information

        // ���� ������ �������� ��
        public static Map Current { get; private set; } = null;

        // ��������Ʈ ��ġ ����
        public List<Transform> SpawnPoints;

        // ���� ���� ī��Ʈ Ÿ�̸�
        [Networked] public TickTimer StartTimer { get; private set; }

        // ���� �ε� �� ���ð�
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