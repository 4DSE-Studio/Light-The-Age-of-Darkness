using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Legacy
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(Rigidbody)),
     RequireComponent(typeof(Patrol)), RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int IsDead = Animator.StringToHash(nameof(IsDead));

        public Camera _visionCamera;
        public LayerMask _playerLayer;

        public float _pickupRadius = 2f;
        public Transform _holdPosition;

        public float _attackDistance = 10f;
        public float _attackDamage = 10f;
        public float _attackInterval = 1f;
        public Transform _player;

        public float _attackCooldown = 1f;

        [SerializeField] private BasicHealth _health;
        [SerializeField] private EnemyAttack _attack;

        private Rigidbody _rb;
        private ExplosiveObject _pickedObject;
        private Joint _joint;

        private bool _isSeeingPlayer;
        private bool _isHoldingObject;
        private bool _isAttackingPlayer;
        private float _lastAttackedAt = -9999f;
        private NavMeshAgent _agent;

        private Patrol _patrol;
        private Rigidbody _rigidbody;
        private bool _isStop;

        private Animator _animator;

        public BasicHealth Health => _health;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _patrol = GetComponent<Patrol>();
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Health.IsDied)
                return;

            if (_isStop)
                return;

            CheckVision();

            if (_isSeeingPlayer)
            {
                _patrol.StopPatrol();
                transform.LookAt(_player.position);

                if (Time.time > _lastAttackedAt + _attackCooldown)
                {
                    if (_isHoldingObject)
                        ThrowObject();

                    if (_attack.IsCaptureEnemy)
                        AttackPlayer();
                    else
                        _isAttackingPlayer = false;

                    _lastAttackedAt = Time.time;
                }

                if (!_isHoldingObject)
                    FindObject();

                _agent.SetDestination(_attack.IsCaptureEnemy ? transform.position : _player.position);
            }
            else
            {
                _patrol.StartPatrol();
            }
        }

        private void OnEnable()
        {
            Health.Died += HealthOnDied;
        }

        private void OnDisable()
        {
            Health.Died -= HealthOnDied;
        }

        private void CheckVision()
        {
            Vector3 playerPoint = _visionCamera.WorldToViewportPoint(_player.position);

            if (playerPoint.x is >= 0f and <= 1f && playerPoint.y is >= 0f and <= 1f)
            {
                Ray ray = _visionCamera.ViewportPointToRay(playerPoint);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, _visionCamera.farClipPlane, _playerLayer))
                    _isSeeingPlayer = true;
                else
                    _isSeeingPlayer = false;
            }
            else
            {
                _isSeeingPlayer = false;
            }
        }

        private void FindObject()
        {
            const int MaxColliders = 5;
            Collider[] hitColliders = new Collider[MaxColliders];

            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, _pickupRadius, hitColliders);

            float minDistance = Mathf.Infinity;
            ExplosiveObject closestObject = null;

            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].TryGetComponent(out ExplosiveObject explosiveObject) == false
                    || explosiveObject.CanPickUp(Power.Dark) == false)
                    continue;

                if (explosiveObject.transform.parent is not null)
                    continue;

                float distance = Vector3.Distance(transform.position, explosiveObject.transform.position);

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestObject = explosiveObject;
            }

            if (closestObject == null)
                return;

            PickupObject(closestObject);
        }

        private void PickupObject(ExplosiveObject explosiveObject)
        {
            _isHoldingObject = true;

            _pickedObject = explosiveObject;

            _pickedObject.ObjectRigidbody.isKinematic = true;
            _pickedObject.ObjectRigidbody.useGravity = false;
            _pickedObject.ObjectRigidbody.drag = 0;
            _pickedObject.ObjectRigidbody.angularDrag = 0;

            Transform transform1 = _pickedObject.transform;
            StartCoroutine(Utils.MoveToTarget(transform1, _holdPosition.position, 0.2f));
            transform1.SetParent(_holdPosition);

            transform1.localEulerAngles = Vector3.zero;
        }

        private void ThrowObject()
        {
            if (_pickedObject is null)
                return;

            _pickedObject.transform.SetParent(null);
            _pickedObject.ObjectRigidbody.isKinematic = false;
            _pickedObject.ObjectRigidbody.useGravity = true;
            _pickedObject.ObjectRigidbody.drag = 0.2f;
            _pickedObject.ObjectRigidbody.angularDrag = 0.5f;

            StartCoroutine(_pickedObject.Throw(_player.position));

            _pickedObject = null;

            _isHoldingObject = false;
        }

        private void AttackPlayer()
        {
            if (!_isAttackingPlayer)
            {
                _isAttackingPlayer = true;

                StartCoroutine(AttackPlayerCoroutine());
            }
        }

        private IEnumerator AttackPlayerCoroutine()
        {
            while (_isAttackingPlayer)
            {
                transform.LookAt(_player);
                _attack.Attack();

                yield return new WaitForSeconds(_attackInterval);
            }
        }

        public void AffectExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier)
        {
            _isStop = true;
            _patrol.StopPatrol();

            _agent.ActivateCurrentOffMeshLink(false);
            _agent.enabled = false;

            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;

            _rigidbody.AddExplosionForce(explosionForce * 5, explosionPosition, explosionRadius, upwardsModifier);

            StartCoroutine(EnableAgentAfterDelay());
        }

        public void Stop()
        {
            _agent.SetDestination(transform.position);
            _isStop = true;
            _patrol.StopPatrol();
            _isAttackingPlayer = false;
        }

        private IEnumerator EnableAgentAfterDelay()
        {
            yield return new WaitForSeconds(1.0f);

            while (_rigidbody.velocity.magnitude > 0.1f)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (_rigidbody == null)
                yield break;

            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            _agent.enabled = true;
            _agent.ActivateCurrentOffMeshLink(true);
            _isStop = false;
        }

        private void HealthOnDied()
        {
            _animator.SetBool(IsDead, true);
            Stop();
        }
    }
}