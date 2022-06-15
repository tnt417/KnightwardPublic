using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour, IDamageable
{

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;
        OnDamaged?.Invoke();
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public event IDamageable.DamageAction OnDamaged;

    public void Awake()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }
}
