using UnityEngine;
using UnityEngine.InputSystem;

namespace UltimateCartFights.Game {
    public class CartInput : MonoBehaviour {

        #region Cart Input Method

        [SerializeField] private InputAction accelerataion;
        [SerializeField] private InputAction steer;
        [SerializeField] private InputAction dash;

        private Gamepad gamepad;
        private bool wasDashed;

        public InputData GetInput() {
            // 현재 연결된 컨트롤러 정보를 가져옴
            gamepad = Gamepad.current;
            
            InputData input = new InputData();

            // 가속, 회전 입력 정보 저장
            input.Acceleration = accelerataion.ReadValue<float>();
            input.Steer = steer.ReadValue<float>();

            // 대쉬키 입력 여부 저장
            input.IsDash = dash.IsPressed();
            input.IsDashDown = (!wasDashed) && (input.IsDash);
            
            wasDashed = input.IsDash;
            
            return input;
        }

        #endregion

        #region Unity LifeCycle Method

        private void Awake() {
            accelerataion = accelerataion.Clone();
            steer = steer.Clone();
            dash = dash.Clone();

            accelerataion.Enable();
            steer.Enable();
            dash.Enable();
        }

        private void OnDestroy() {
            accelerataion.Disable();
            steer.Disable();
            dash.Disable();
        }

        #endregion
    }

    public struct InputData {

        // 가속 (전/후진)
        private int _acceleration;
        public float Acceleration {
            get { return _acceleration * 0.001f; }
            set { _acceleration = (int) (value * 1000); }
        }

        // 회전 (좌/우회전)
        private int _steer;
        public float Steer {
            get { return _steer * 0.001f; }
            set { _steer = (int) (value * 1000); }
        }

        // 대쉬
        public bool IsDash;        // 대쉬키가 눌려져 있는지 확인
        public bool IsDashDown;    // 현재 프레임에 대쉬키가 눌려졌는지 확인
    }
}