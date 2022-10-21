using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game
{
    public class DestructableObject : MonoBehaviour, IDamageable
    {
        public UnityEvent onDestroy;
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
        public bool IsTangible { get; } = true;

        public float ApplyDamage(float damage, out bool successful, bool ignoreInvincibility = false)
        {
            successful = true;
            hitsRemaining--;
            animator.Play(hitAnimation);
            if(hitsRemaining <= 0) Die();
            return damage;
        }

        public void Die()
        {
            ObjectSpawner.SpawnMoney(moneyDrop, transform.position, GetComponentInParent<NetworkIdentity>());
            onDestroy?.Invoke();
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChanged;
        public event IDamageable.HealthAction OnHeal;
        public event IDamageable.HealthAction OnHurt;
        public event IDamageable.HealthAction OnDeath;
    }
}
