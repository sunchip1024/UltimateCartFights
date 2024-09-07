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
            // ���� ����� ��Ʈ�ѷ� ������ ������
            gamepad = Gamepad.current;
            
            InputData input = new InputData();

            // ����, ȸ�� �Է� ���� ����
            input.Acceleration = accelerataion.ReadValue<float>();
            input.Steer = steer.ReadValue<float>();

            // �뽬Ű �Է� ���� ����
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

        // ���� (��/����)
        private int _acceleration;
        public float Acceleration {
            get { return _acceleration * 0.001f; }
            set { _acceleration = (int) (value * 1000); }
        }

        // ȸ�� (��/��ȸ��)
        private int _steer;
        public float Steer {
            get { return _steer * 0.001f; }
            set { _steer = (int) (value * 1000); }
        }

        // �뽬
        public bool IsDash;        // �뽬Ű�� ������ �ִ��� Ȯ��
        public bool IsDashDown;    // ���� �����ӿ� �뽬Ű�� ���������� Ȯ��
    }
}