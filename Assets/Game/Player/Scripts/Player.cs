using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour, IDamageable
{
    public PlayerMovement playerMovement;
    public PlayerCombat playerCombat;
    public PlayerAnimator playerAnimator;

    #region IDamageable
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
        Timer.Stop();
    }

    public event IDamageable.DamageAction OnDamaged;

    #endregion

    public void Awake()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }
    
}