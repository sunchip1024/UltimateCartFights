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

            // ������ �Է� ������ ���� �÷��̾ ã�� PlayerID�� �ʱ�ȭ
            ClientPlayer client = ClientPlayer.Players.FirstOrDefault(x => {
                return x.Object.InputAuthority == Object.InputAuthority;
            });

            // �÷��̾� ID �ʱ�ȭ
            PlayerID = client.PlayerID;

            // īƮ Ŀ���͸���¡
            CartCustom custom = GetComponent<CartCustom>();
            custom.SetCharacter(client.Character);
            custom.SetColor(client.CartColor);

            // Local �� ī�Ŷ� ����
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

            // ���ŵǴ� īƮ�� ī�޶� ���� ���̶�� ���� īƮ�� ����
            if (CartCamera.IsFocused(this))
                CartCamera.SetTarget(Carts.First());
        }

        #endregion

        #region Cart Control Method

        private void RefreshAppliedSpeed() {
            // ���� ��ǥ�迡���� �ӵ� ���͸� ���
            Vector3 local = transform.InverseTransformDirection(rigidbody.velocity);

            // ���� ��ǥ�迡���� z�� ����(= front)�� �ӵ��� ����
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
            // ��� Ű�� �Է����� �� ��� ��ų�� ��Ÿ���� �ƴ϶�� Ÿ�̸� ����
            if(input.IsDashDown && dashCoolTimer.ExpiredOrNotRunning(Runner) && CanDrive()) {
                dashTimer = TickTimer.CreateFromSeconds(Runner, DASH_DURATION);
                dashCoolTimer = TickTimer.CreateFromSeconds(Runner, DASH_COOLTIME);
            }

            // �����ִ� ��� �����ð� ���
            float dashRemained = 0;
            if (!dashTimer.ExpiredOrNotRunning(Runner))
                dashRemained = (float) dashTimer.RemainingTime(Runner);

            // ��� �����ð��� �����ִٸ� �ӵ� ����
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
                // �浹�� ������ ���
                Vector3 bounceDir = collision.impulse.normalized;

                // ��ݷ� ���
                float bouncePower = Mathf.Max(collision.impulse.magnitude, 2f);

                // �浹 �ݴ� �������� ���� ����
                rigidbody.AddForce((-1) * bounceDir * bouncePower * BUMP_FORCE, ForceMode.Impulse);

                // �浹 �� ���� �浹 ���� Ÿ�̸� ����
                bumpTimer = TickTimer.CreateFromSeconds(Runner, BUMP_COOLTIME);

                // Host�� �浹�� īƮ �� �ӵ��� ���� ������ ��� ����
                if(GameLauncher.IsHost()) {
                    // �浹�� ��� īƮ�� ���� �ӵ� ������
                    CartController other = collision.gameObject.GetComponent<CartController>();
                    float speed = Mathf.Abs(other.appliedSpeed);

                    // ��� īƮ�� �ӵ��� ����Ͽ� ������ ����
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