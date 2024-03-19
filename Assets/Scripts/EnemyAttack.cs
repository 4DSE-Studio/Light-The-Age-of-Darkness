using Legacy;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private ParticleSystem _attackEffect;
    [SerializeField] private float _damage = 20f;

    private bool _isLeft = true;
    public Player AttackedEnemy { get; private set; }

    public bool IsCaptureEnemy => AttackedEnemy != null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player enemy) == false)
            return;

        if (AttackedEnemy == null)
            AttackedEnemy = enemy;
        else if (Vector3.Distance(transform.position, AttackedEnemy.transform.position) < Vector3.Distance(transform.position, enemy.transform.position))
            AttackedEnemy = enemy;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player enemy) == false)
            return;

        if (enemy == AttackedEnemy)
            AttackedEnemy = null;
    }

    public void Attack()
    {
        if (AttackedEnemy != null)
        {
            AttackedEnemy.Health.TakeDamage(_damage, Power.Dark);
        }

        _attackEffect.Play(true);

        _isLeft = !_isLeft;
    }
}