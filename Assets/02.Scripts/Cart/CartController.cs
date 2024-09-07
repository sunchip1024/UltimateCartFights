using UnityEngine;

namespace UltimateCartFights.Game {
    public class CartController : MonoBehaviour {

        #region Cart Properties

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

        private void Awake() {
            CartCamera.SetTarget(this);
        }

        private void FixedUpdate() {
            input = cartInput.GetInput();

            Move(input);
            Steer(input);
        }

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