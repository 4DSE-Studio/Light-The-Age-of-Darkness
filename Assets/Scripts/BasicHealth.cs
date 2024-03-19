using System;
using Legacy;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasicHealth : MonoBehaviour
{
    [SerializeField, Min(0)] private float _maxHealth;
    [SerializeField, ReadOnly] private float _health;
    public UnityEvent _died;

    public bool IsDied { get; private set; }

    public float MaxHealth => _maxHealth;

    public float Health
    {
        get => _health;
        set => _health = value;
    }

    private void Awake()
    {
        Health = _maxHealth;
    }

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public void TakeDamage(float damage, Power type)
    {
        Debug.Log($"Try take damage {damage} with {type} to {name}");

        bool isDamage = CanDamage(damage, type);

        if (isDamage == false)
        {
            Debug.LogWarning($"Damage {damage} is not correct", this);
            return;
        }

        ApplyDamage(damage, type);

        if (Health > 0)
            return;

        IsDied = true;
        Died?.Invoke();
        _died?.Invoke();
    }

    protected virtual bool CanDamage(float damage, Power type) => IsDied == false && damage > 0 && type != Power.None;

    protected virtual void ApplyDamage(float damage, Power type)
    {
        Health = Mathf.Clamp(Health - damage, 0, _maxHealth);
        HealthChanged?.Invoke(_health, _maxHealth);
    }

    protected virtual void Heal(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"Heal amount {amount} is not correct", this);
            return;
        }

        Health = Mathf.Clamp(Health + amount, 0, _maxHealth);
    }

    [ContextMenu("Die")]
    public void Die()
    {
        IsDied = true;
        Died?.Invoke();
    }
}