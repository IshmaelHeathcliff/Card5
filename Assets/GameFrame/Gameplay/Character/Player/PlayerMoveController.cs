using UnityEngine;
using UnityEngine.InputSystem;
using InputSystem = Core.InputSystem;

namespace Gameplay.Character.Player
{
    public class PlayerMoveController : MoveController
    {
        [SerializeField] float _speed = 10;
        [SerializeField, Range(1, 20f)] float _acceleration = 10f;

        bool _isMoving;
        PlayerInput.PlayerActions _playerInput;

        static readonly int Walking = Animator.StringToHash("Walking");

        protected override void OnInit()
        {
            base.OnInit();
            Speed = _speed;
        }


        void Move()
        {
            Vector2 targetVelocity = _isMoving ? Direction * _speed : Vector2.zero;

            Vector2 currentVelocity;
            if ((Rigidbody.linearVelocity - targetVelocity).sqrMagnitude > 0.01f)
            {
                currentVelocity = Vector2.Lerp(Rigidbody.linearVelocity, targetVelocity,
                    _acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = targetVelocity;
            }

            Rigidbody.linearVelocity = currentVelocity;
        }

        void MoveAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Face(context.ReadValue<Vector2>());
                Animator.SetBool(Walking, true);
                _isMoving = true;
            }

            if (context.canceled)
            {
                Animator.SetBool(Walking, false);
                _isMoving = false;
            }
        }

        void RegisterActions()
        {
            _playerInput.Move.performed += MoveAction;
            _playerInput.Move.canceled += MoveAction;
        }

        void UnregisterActions()
        {
            _playerInput.Move.performed -= MoveAction;
            _playerInput.Move.canceled -= MoveAction;
        }


        void OnEnable()
        {
            _playerInput = this.GetSystem<InputSystem>().PlayerActionMap;
            RegisterActions();
            _playerInput.Enable();

        }

        void OnDisable()
        {
            UnregisterActions();
            _playerInput.Disable();
        }

        void FixedUpdate()
        {
            Move();
        }
    }
}
