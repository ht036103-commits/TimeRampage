using System;
using UnityEngine;

public sealed class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maximumHealth = 100;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;
    public bool IsDead => isDead;

    public event Action<int, int> HealthChanged;
    public event Action<int> Damaged;
    public event Action Died;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(
            0,
            currentHealth - amount
        );

        Damaged?.Invoke(amount);

        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(
            maximumHealth,
            currentHealth + amount
        );

        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maximumHealth;

        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Died?.Invoke();
    }

    private void OnValidate()
    {
        maximumHealth = Mathf.Max(1, maximumHealth);
    }
}