using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game
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
        public int MaxHealth => hitsRemaining;
        public float CurrentHealth => hitsRemaining;
        public bool IsInvulnerable => false;
        
        public float ApplyDamage(float damage)
        {
            hitsRemaining--;
            animator.Play(hitAnimation);
            if(hitsRemaining <= 0) Die();
            return damage;
        }

        public void Die()
        {
            //TODO ObjectSpawner.SpawnMoney(moneyDrop, transform.position);
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChanged;
        public event IDamageable.HealthAction OnHeal;
        public event IDamageable.HealthAction OnHurt;
        public event IDamageable.HealthAction OnDeath;
    }
}
