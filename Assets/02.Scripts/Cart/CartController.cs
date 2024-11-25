using Fusion;
using System.Collections.Generic;
using System.Linq;
using UltimateCartFights.Network;
using UnityEngine;

namespace UltimateCartFights.Game {
    public class CartController : NetworkBehaviour {

        #region Cart Properties

        /* Cart List */

        public static readonly List<CartController> Carts = new List<CartController>();

        public static CartController Local { get; private set; } = null;

        /* Game State Property */

        [Networked] public int PlayerID { get; private set; }

        public bool IsGameStarted;

        /* Damage Property */

        public const float DAMAGE_LIMIT = 999.9f;

        public const float BUMP_DAMAGE = 20f;

        [Networked] public float damage { get; private set; } = 0;

        /* Dash Property */

        public const float DASH_DURATION = 3f;

        public const float DASH_COOLTIME = 15f;

        [Networked] public TickTimer dashTimer { get; private set; }

        [Networked] public TickTimer dashCoolTimer { get; private set; }

        /* Cart Controller Property */

        public const float NORMAL_MAX_SPEED = 7;

        public const float DASH_MAX_SPEED = 15;

        public const float ACCELERATION = 0.5f;

        public const float DECELARATION = 0.75f;

        public const float STEER_AMOUNT = 25f;

        [Networked] public InputData input { get; private set; }

        [Networked] public float maxSpeed { get; private set; } = NORMAL_MAX_SPEED;

        [Networked] public float appliedSpeed { get; private set; } = 0;

        /* Collider Property */

        public const float BUMP_FORCE = 3f;

        public const float BUMP_COOLTIME = 0.3f;

        [Networked] public TickTimer bumpTimer { get; private set; }

        [SerializeField, Layer] private int CART_LAYER;

        [SerializeField] private Rigidbody rigidbody;

        #endregion

        #region Cart LifeCycle Method

        private ChangeDetector changeDetector;

        public override void Spawned() {
            base.Spawned();

            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            Carts.Add(this);

            // 동일한 입력 권한을 가진 플레이어를 찾아 PlayerID를 초기화
            ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => {
                return x.Object.InputAuthority == Object.InputAuthority;
            });

            // 플레이어 ID 초기화
            PlayerID = client.PlayerID;

            // 카트 커스터마이징
            CartCustom custom = GetComponent<CartCustom>();
            custom.SetCharacter(client.Character);
            custom.SetColor(client.CartColor);

            // Local 및 카매라 설정
            if (Object.HasInputAuthority) {
                Local = this;
                CartCamera.SetTarget(this);

                Debug.Log($"[ * DEBUG * ] Camera Target : {PlayerID}");
            }

            IsGameStarted = true;
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            if (GetInput(out InputData current))
                this.input = current;

            if (CanDrive()) {
                Move(input);
                Steer(input);
            } else {
                RefreshAppliedSpeed();
            }

            Dash(input);
        }

        public override void Render() {
            base.Render();

            foreach(string change in changeDetector.DetectChanges(this)) {
                switch(change) {
                    case nameof(damage):
                        Debug.Log($"[ * Debug * ] Player {PlayerID} Total Damage : {damage}");
                        GameLauncher.OnGetDamaged(PlayerID, damage);
                        break;
                }
            }
        }

        private void OnDisable() {
            if (Local == this)
                Local = null;

            Carts.Remove(this);

            // 제거되는 카트를 카메라가 추적 중이라면 다음 카트를 추적
            if (CartCamera.IsFocused(this))
                CartCamera.SetTarget(Carts.First());
        }

        #endregion

        #region Cart Control Method

        private void RefreshAppliedSpeed() {
            // 로컬 좌표계에서의 속도 벡터를 계산
            Vector3 local = transform.InverseTransformDirection(rigidbody.velocity);

            // 로컬 좌표계에서의 z축 성분(= front)를 속도로 지정
            appliedSpeed = local.z;
        }

        private void Move(InputData input) {
            if (input.Acceleration > 0)
                appliedSpeed = Mathf.Lerp(appliedSpeed, maxSpeed, ACCELERATION * Time.fixedDeltaTime);
            else if (input.Acceleration < 0)
                appliedSpeed = Mathf.Lerp(appliedSpeed, -NORMAL_MAX_SPEED, ACCELERATION * Time.fixedDeltaTime);
            else
                appliedSpeed = Mathf.Lerp(appliedSpeed, 0, DECELARATION * Time.fixedDeltaTime);

            Vector3 velocity = (rigidbody.rotation * Vector3.forward * appliedSpeed) + (Vector3.up * rigidbody.velocity.y);
            rigidbody.velocity = velocity;
        }

        private void Steer(InputData input) {
            Vector3 targetRot = rigidbody.rotation.eulerAngles + Vector3.up * STEER_AMOUNT * input.Steer;

            Vector3 rot = Vector3.Lerp(rigidbody.rotation.eulerAngles, targetRot, 3 * Time.fixedDeltaTime);

            rigidbody.MoveRotation(Quaternion.Euler(rot));
        }

        private void Dash(InputData input) {
            // 대시 키를 입력했을 때 대시 스킬이 쿨타임이 아니라면 타이머 실행
            if(input.IsDashDown && dashCoolTimer.ExpiredOrNotRunning(Runner) && CanDrive()) {
                dashTimer = TickTimer.CreateFromSeconds(Runner, DASH_DURATION);
                dashCoolTimer = TickTimer.CreateFromSeconds(Runner, DASH_COOLTIME);
            }

            // 남아있는 대시 유지시간 계산
            float dashRemained = 0;
            if (!dashTimer.ExpiredOrNotRunning(Runner))
                dashRemained = (float) dashTimer.RemainingTime(Runner);

            // 대시 유지시간이 남아있다면 속도 가속
            if(dashRemained > 0) {
                maxSpeed = DASH_MAX_SPEED;
                appliedSpeed = Mathf.Lerp(appliedSpeed, maxSpeed, Runner.DeltaTime);
            } else {
                maxSpeed = NORMAL_MAX_SPEED;
            }
        }

        private void OnCollisionStay(Collision collision) {
            if (!bumpTimer.ExpiredOrNotRunning(Runner)) return;

            int layer = collision.gameObject.layer;
            if (layer == CART_LAYER) {
                // 충돌한 방향을 계산
                Vector3 bounceDir = collision.impulse.normalized;

                // 충격량 계산
                float bouncePower = Mathf.Max(collision.impulse.magnitude, 2f);

                // 충돌 반대 방향으로 힘을 가함
                rigidbody.AddForce((-1) * bounceDir * bouncePower * BUMP_FORCE, ForceMode.Impulse);

                // 충돌 후 연속 충돌 방지 타이머 실행
                bumpTimer = TickTimer.CreateFromSeconds(Runner, BUMP_COOLTIME);

                // Host만 충돌한 카트 간 속도를 통해 데미지 계산 실행
                if(GameLauncher.IsHost()) {
                    // 충돌한 상대 카트의 현재 속도 가져옴
                    CartController other = collision.gameObject.GetComponent<CartController>();
                    float speed = Mathf.Abs(other.appliedSpeed);

                    // 상대 카트의 속도에 비례하여 데미지 누적
                    damage += speed * BUMP_DAMAGE;
                }
            }
        }

        private bool CanDrive() {
            if (!IsGameStarted) return false;
            if (!Map.IsGameStarted()) return false;
            if (!bumpTimer.ExpiredOrNotRunning(Runner)) return false;
            return true;
        }

        #endregion

        #region Others 

        public float GetDashCooltime() {
            if (dashCoolTimer.ExpiredOrNotRunning(Runner))
                return 0;
            else
                return (float) dashCoolTimer.RemainingTime(Runner);
        }

        #endregion
    }
}