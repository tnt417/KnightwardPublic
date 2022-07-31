using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev
{
    public class DestructableObject : MonoBehaviour, IDamageable
    {
        public int hitsRemaining;
        
        public int moneyDrop;
        [SerializeField] private Animator animator;
        [SerializeField] private string hitAnimation;

        public Team Team => Team.Enemy;
        public float DamageMultiplier { get; } = 1f;
        public float HealMultiplier { get; } = 1f;
        public int MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsInvulnerable { get; }
        
        public void ApplyDamage(float damage)
        {
            hitsRemaining--;
            animator.Play(hitAnimation);
            if(hitsRemaining <= 0) Die();
        }

        public void Die()
        {
            PickupSpawner.SpawnMoney(moneyDrop, transform.position);
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChanged;
        public event IDamageable.HealthAction OnHeal;
        public event IDamageable.HealthAction OnHurt;
        public event IDamageable.HealthAction OnDeath;
    }
}
