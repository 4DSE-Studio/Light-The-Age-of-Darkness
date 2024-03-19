using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Legacy
{
    [RequireComponent(typeof(Rigidbody))]
    public class ExplosiveObject : MonoBehaviour
    {
        [SerializeField] private float _upwardsModifier = 5f;

        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private float _explosionForce = 10f;
        [SerializeField] private float _explosionDamage = 20f;

        [SerializeField] private ParticleSystem _darkExplosionEffect;
        [SerializeField] private ParticleSystem _lightExplosionEffect;

        [SerializeField] private float _duration = 20f;

        [SerializeField] private ChangePower _changePower;

        public Rigidbody ObjectRigidbody { get; private set; }

        public bool IsThrowing { get; private set; }
        public bool IsExplodes { get; private set; }
        public Power Power => _changePower.CurrentPower;

        private void Start()
        {
            ObjectRigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool isPlayer = collision.gameObject.TryGetComponent(out Player _);

            if (isPlayer)
            {
                if (_changePower.CurrentPower == Power.Dark)
                    Explode();

                return;
            }

            bool isEnemy = collision.gameObject.TryGetComponent(out EnemyAI _);

            if (isEnemy)
            {
                if (_changePower.CurrentPower == Power.Light)
                    Explode();

                return;
            }

            if (IsThrowing == false)
                return;

            Explode();
        }

        public bool CanPickUp(Power power) => IsThrowing == false && IsExplodes == false && _changePower.CurrentPower == power && _changePower.IsChanging == false;

        public event Action Exploded;

        public IEnumerator Throw(Vector3 target)
        {
            IsThrowing = true;

            Vector3 startPosition = transform.position;

            float distance = Vector3.Distance(startPosition, target);

            yield return StartCoroutine(Utils.MoveToTarget(transform, target, distance / _duration));
        }

        private void Explode(float delay = 0f)
        {
            StartCoroutine(DoExplode(delay));
        }

        private IEnumerator DoExplode(float delay = 0f)
        {
            IsExplodes = true;

            yield return new WaitForSeconds(delay);

            PlayEffect();

            const int MaxColliders = 10;
            Collider[] hitColliders = new Collider[MaxColliders];

            int collidersCount = Physics.OverlapSphereNonAlloc(transform.position,
                _explosionRadius,
                hitColliders,
                LayerMask.GetMask("Player", "Enemy", "ExplosiveObject"));

            for (int i = 0; i < collidersCount; i++)
            {
                Collider hitCollider = hitColliders[i];

                if (hitCollider.TryGetComponent(out ExplosiveObject explosiveObject) && explosiveObject.IsExplodes == false && explosiveObject.IsThrowing == false)
                {
                    explosiveObject.Explode(Random.Range(0.05f, 0.2f));
                    continue;
                }

                if (hitCollider.TryGetComponent(out Rigidbody component))
                    component.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _upwardsModifier);

                TakeDamage(hitCollider);
            }

            Destroy(gameObject);

            Exploded?.Invoke();

            IsExplodes = false;
        }

        private void PlayEffect()
        {
            ParticleSystem explosionEffect = _changePower.CurrentPower switch
            {
                Power.Light => _lightExplosionEffect,
                Power.Dark => _darkExplosionEffect,
                var _ => null
            };

            ParticleSystem effect = Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(effect.gameObject, effect.main.duration);
        }

        private void TakeDamage(Component hitCollider)
        {
            BasicHealth health = hitCollider.GetComponentInChildren<BasicHealth>();

            if (health != null)
                health.TakeDamage(_explosionDamage, _changePower.CurrentPower);

            switch (_changePower.CurrentPower)
            {
                case Power.Dark when hitCollider.TryGetComponent(out Player player):
                    player.AffectExplosion(_explosionForce, transform.position, _explosionRadius, _upwardsModifier);
                    break;

                case Power.Light when hitCollider.TryGetComponent(out EnemyAI enemy):
                    enemy.AffectExplosion(_explosionForce, transform.position, _explosionRadius, _upwardsModifier);
                    break;

                case Power.None:
                default:
                    break;
            }
        }
    }
}