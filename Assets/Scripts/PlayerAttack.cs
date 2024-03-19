using Legacy;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private ParticleSystem _attackEffect;

    private bool _isLeft = true;
    private bool _isSaveEnemy;

    private bool _isLeave;
    public EnemyAI AttackedEnemy { get; private set; }

    public bool IsCaptureEnemy => AttackedEnemy != null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Attack();

        //if (IsCaptureEnemy && Input.GetKeyDown(KeyCode.Z))
        if (Input.GetKeyDown(KeyCode.Z))
            _isSaveEnemy = !_isSaveEnemy;

        if (_isLeave && _isSaveEnemy == false)
            AttackedEnemy = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EnemyAI enemy) == false)
            return;

        if (AttackedEnemy != null
            && Vector3.Distance(transform.position, AttackedEnemy.transform.position)
            >= Vector3.Distance(transform.position, enemy.transform.position))
            return;

        AttackedEnemy = enemy;
        enemy.Health._died.AddListener(HealthOnDied);
        _isLeave = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EnemyAI enemy) == false)
            return;

        if (enemy != AttackedEnemy)
            return;

        _isLeave = true;

        if (_isSaveEnemy)
            return;

        AttackedEnemy = null;
    }

    private void HealthOnDied()
    {
        _isSaveEnemy = false;
        AttackedEnemy = null;
    }

    private void Attack()
    {
        if (AttackedEnemy != null)
            AttackedEnemy.Health.TakeDamage(20f, Power.Light);

        _attackEffect.Play(true);

        _isLeft = !_isLeft;
    }
}