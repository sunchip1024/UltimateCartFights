using UnityEngine;

namespace UltimateCartFights.Game {
    public class CartCamera : MonoBehaviour {

        public const float MIN_FOV = 45f;
        public const float MAX_FOV = 90f;

        public const float MIN_SPEED = 3f;
        public const float MAX_SPEED = 10f;

        public const float MIN_DISTANCE = 0f;
        public const float MAX_DISTANCE = 2.5f;

        public const float SMOOTH_SPEED = 0.125f;

        public readonly Vector3 CAMERA_OFFSET = new Vector3(0, 0.5f, -1.5f);
        public readonly Vector3 LOOK_OFFSET = new Vector3(0, 0.5f, 0);

        private static CartController target = null;

        private Camera mainCamera = null;

        private void Awake() {
            mainCamera = GetComponent<Camera>();
        }

        private void FixedUpdate() {
            if (target == null) return;

            float speed = Mathf.Abs(target.appliedSpeed);

            Vector3 targetPosition = target.transform.position + target.transform.rotation * CAMERA_OFFSET;
            targetPosition -= target.transform.forward * GetDistance(speed);

            Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, SMOOTH_SPEED);
            transform.position = smoothPosition;

            Vector3 lookPosition = target.transform.position + target.transform.rotation * LOOK_OFFSET;
            transform.LookAt(lookPosition);

            float targetFOV = GetFOV(speed);
            mainCamera.fieldOfView = targetFOV;
        }

        public static void SetTarget(CartController _target) => target = _target;

        public static bool IsFocused(CartController _target) => target == _target;

        private float GetFOV(float speed) {
            if (speed < MIN_SPEED) return MIN_FOV;
            if (speed >= MAX_SPEED) return MAX_FOV;

            float proportion = Mathf.InverseLerp(MIN_SPEED, MAX_SPEED, speed);
            return Mathf.Lerp(MIN_FOV, MAX_FOV, proportion);
        }

        private float GetDistance(float speed) {
            if (speed < MIN_SPEED) return MIN_DISTANCE;
            if (speed >= MAX_SPEED) return MAX_DISTANCE;

            float proportion = Mathf.InverseLerp(MIN_SPEED, MAX_SPEED, speed);
            return Mathf.Lerp(MIN_DISTANCE, MAX_DISTANCE, proportion);
        }
    }
}