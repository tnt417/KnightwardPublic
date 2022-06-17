using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour, IDamageable
{
    public void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    
    #region IDamageable

    public Team team => Team.Player;
    public int MaxHealth => 1000;
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
        Timer.Stop();
        Destroy(gameObject);
    }

    public event IDamageable.DamageAction OnDamaged;
    #endregion
}
