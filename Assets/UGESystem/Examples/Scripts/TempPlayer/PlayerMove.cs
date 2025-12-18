using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UGESystem
{
    /// <summary>
    /// Temporary character controller script providing basic third-person movement based on camera direction for testing purposes.
    /// 테스트 목적으로 카메라 방향에 기반한 기본적인 3인칭 이동을 제공하는 임시 캐릭터 컨트롤러 스크립트입니다.
    /// </summary>
    public class PlayerMove : MonoBehaviour
    {
        /// <summary>
        /// The camera transform used for determining movement direction.
        /// 움직임 방향 결정에 사용되는 카메라 트랜스폼입니다.
        /// </summary>
        public Transform cam;
        /// <summary>
        /// The current movement speed of the character.
        /// 캐릭터의 현재 이동 속도입니다.
        /// </summary>
        public float _speed;
        /// <summary>
        /// The time taken to smooth the character's rotation when changing direction.
        /// 방향 전환 시 캐릭터의 회전을 부드럽게 만드는 데 걸리는 시간입니다.
        /// </summary>
        public float _turnSmoothTime = 0.3f;

        const float MAXSPEED = 10.0f;
        float _turnSmoothVelocity;
        float _velocityY = 0f;

        private InputSystem_Actions _playerInputActions;
        private Vector2 _moveInput;

        Rigidbody _rigidbody;
        Vector3 _move;
        Vector3 _lookDirection = new(0, 0, 0);

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInputActions = new @InputSystem_Actions();
            _playerInputActions.Player.Enable();

            _playerInputActions.Player.Move.performed += OnMovePerformed;
            _playerInputActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }

        private void OnEnable()
        {
            if (_playerInputActions != null)
            {
                _playerInputActions.Player.Enable();
            }
        }

        private void OnDisable()
        {
            if (_playerInputActions != null)
            {
                _playerInputActions.Player.Move.performed -= OnMovePerformed;
                _playerInputActions.Player.Move.canceled -= OnMoveCanceled;
                _playerInputActions.Player.Disable();
            }
        }

        void FixedUpdate()
        {
            _move = new(_moveInput.x, _velocityY, _moveInput.y);

            _lookDirection = _move.normalized;

            if (_lookDirection.magnitude >= 0.1f)
            {
                _speed = Mathf.Clamp(_move.magnitude * MAXSPEED, 0.0f, MAXSPEED);
                Move();
            }
            else
            {
                _speed = 0.0f;
            }
        }



        private void Move()
        {
            float targetAngle = Mathf.Atan2(_lookDirection.x, _lookDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _rigidbody.MovePosition(_rigidbody.position + moveDir.normalized * Time.deltaTime * _lookDirection.magnitude * _speed);
        }
    }
}

