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

        /* Cart Controller Property */

        public const float NORMAL_MAX_SPEED = 7;

        public const float DASH_MAX_SPEED = 15;

        public const float ACCELERATION = 0.5f;

        public const float DECELARATION = 0.75f;

        public const float STEER_AMOUNT = 25f;

        public InputData input;

        public float maxSpeed = NORMAL_MAX_SPEED;

        public float appliedSpeed = 0;

        [SerializeField] private CartInput cartInput;

        /* Collider Property */

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
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            input = cartInput.GetInput();

            Move(input);
            Steer(input);
        }

        public override void Render() {
            base.Render();
        }

        private void OnDisable() {
            if (Local == this)
                Local = null;

            Carts.Remove(this);

            // 제거되는 카트를 카메라가 추적 중이라면 다음 카트를 추적
            if (CartCamera.IsFocused(this))
                CartCamera.SetTarget(Carts.First());
        }

        /*
        private void Awake() {
            CartCamera.SetTarget(this);
        }

        private void FixedUpdate() {
            input = cartInput.GetInput();

            Move(input);
            Steer(input);
        }
        */

        #endregion

        #region Cart Control Method

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

        #endregion
    }
}