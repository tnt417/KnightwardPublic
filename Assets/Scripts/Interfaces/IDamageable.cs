using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    void ApplyDamage(int damage);
    void Die();
    delegate void DamageAction();
    event DamageAction OnDamaged;
}
