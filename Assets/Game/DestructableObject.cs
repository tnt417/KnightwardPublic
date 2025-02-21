using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
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
            if (_dying)
            {
                successful = false;
                return 0;
            }
            
            successful = true;
            hitsRemaining--;
            animator.Play(hitAnimation);

            if (hitsRemaining <= 0)
            {
                Die();
            }
            return damage;
        }

        private bool _dying = false;

        public void Die()
        {
            var token = new CancellationTokenSource();
            token.RegisterRaiseCancelOnDestroy(this);
            DieTask().AttachExternalCancellation(token.Token);
        }
        
        public async UniTask DieTask()
        {
            _dying = true;
            ObjectSpawner.SpawnMoney(moneyDrop, transform.position, GetComponentInParent<NetworkIdentity>());
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            onDestroy?.Invoke();
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChangedOwner;
        public event IDamageable.HealthAction OnHealOwner;
        public event IDamageable.HealthAction OnHurtOwner;
        public event IDamageable.HealthAction OnDeathOwner;
    }
}
