using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    Team team { get; }
    int MaxHealth { get; }
    int CurrentHealth { get; }
    bool IsAlive { get; }
    void ApplyDamage(int damage);
    void Die();
    delegate void HealthAction();
    event HealthAction OnHealthChanged;
}
