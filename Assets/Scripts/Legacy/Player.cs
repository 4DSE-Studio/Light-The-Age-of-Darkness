using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Legacy
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(Rigidbody)),
     RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        private static readonly int IsDead = Animator.StringToHash(nameof(IsDead));

        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _doubleJumpForce;
        [SerializeField] private float _smoothing;
        [SerializeField] private float _gravityForce = 20;

        [FormerlySerializedAs("_darkExplosionEffect"), SerializeField]
        private ParticleSystem _jumpEffect;

        [SerializeField] private BasicHealth _health;
        [SerializeField] private DeathScreen _deathScreen;
        [SerializeField] private PlayerAttack _playerAttack;

        private CharacterController _characterController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Animator _animator;

        private Vector3 _moveDirection = Vector3.zero;
        private Vector2 _inputMoveDirection;
        private Vector2 _smoothMoveDirection;

        private float _currentAttractionCharacter;
        private bool _canDoubleJump = true;
        private Camera _camera;

        private bool _isStopRotation;
        private bool _isShiftPressed;

        private bool _isDashing;
        private readonly float _dashDuration = 0.5f;
        private readonly float _dashCooldown = 1f;
        private float _currentDashCooldown;
        private float _currentDashDuration;
        public BasicHealth Health => _health;

        private void Awake()
        {
            _camera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
            _characterController = GetComponent<CharacterController>();
            _playerInput = new PlayerInput();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_health.IsDied)
                return;

            _inputMoveDirection = _playerInput.Player.Move.ReadValue<Vector2>();

            SmoothMove();

            float moveSpeed = _moveSpeed;

            _moveDirection.x = _smoothMoveDirection.x * moveSpeed;
            _moveDirection.z = _smoothMoveDirection.y * moveSpeed;

            RotatePlayer();

            if (_characterController.isGrounded)
                _canDoubleJump = true;

            _isShiftPressed = Keyboard.current.leftShiftKey.isPressed;

            if (_playerInput.Player.Jump.triggered)
            {
                if (_characterController.isGrounded)
                {
                    _moveDirection.y = _jumpForce;
                }
                else if (_canDoubleJump)
                {
                    ParticleSystem jumpEffect = Instantiate(_jumpEffect, transform.position, Quaternion.LookRotation(Vector3.up));
                    Destroy(jumpEffect.gameObject, jumpEffect.main.duration);

                    _moveDirection.y = _doubleJumpForce;
                    _canDoubleJump = false;
                }
            }

            if (_isDashing)
            {
                _currentDashDuration -= Time.deltaTime;

                if (_currentDashDuration <= 0)
                {
                    _isDashing = false;
                    _currentDashCooldown = _dashCooldown;
                }
                else
                {
                    Vector3 dashDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.z).normalized;
                    _moveDirection += dashDirection * 10;
                }
            }
            else
            {
                if (_currentDashCooldown > 0)
                {
                    _currentDashCooldown -= Time.deltaTime;
                }
                else if (_isShiftPressed)
                {
                    _isDashing = true;
                    _currentDashDuration = _dashDuration;
                }
            }

            _moveDirection.y -= _gravityForce * Time.deltaTime;
            _characterController.Move(_moveDirection * Time.deltaTime);
        }

        private void OnEnable()
        {
            _playerInput.Enable();
            _health.Died += HealthOnDied;
        }

        private void OnDisable()
        {
            _playerInput.Disable();
            _health.Died -= HealthOnDied;
        }

        private void RotatePlayer()
        {
            if (_playerAttack.IsCaptureEnemy)
            {
                transform.LookAt(_playerAttack.AttackedEnemy.transform);
                return;
            }

            if (_isStopRotation)
                return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetDirection = hit.point - transform.position;
                targetDirection.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        private void HealthOnDied()
        {
            _animator.SetBool(IsDead, true);
            _deathScreen.Show();
            _playerAttack.gameObject.SetActive(false);
        }

        private void SmoothMove()
        {
            _smoothMoveDirection = Vector3.Lerp(_smoothMoveDirection, _inputMoveDirection, Time.deltaTime / _smoothing);
        }

        public void AffectExplosion(float explosionForce, Vector3 transformPosition, float explosionRadius, float upwardsModifier)
        {
            _characterController.enabled = false;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;

            _rigidbody.AddExplosionForce(explosionForce, transformPosition, explosionRadius, upwardsModifier);

            StartCoroutine(EnableMovement());
        }

        private IEnumerator EnableMovement()
        {
            yield return new WaitForSeconds(2f);

            _characterController.enabled = true;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }
    }
}