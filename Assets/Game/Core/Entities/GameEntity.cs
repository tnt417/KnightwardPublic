using System;
using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Entities
{
    public abstract class GameEntity : MonoBehaviour, IDamageable
    {
        public delegate void TargetChangeAction();
        public event TargetChangeAction OnTargetChange;
        public Transform Target { get; private set; } = null;

        private void Start()
        {
            Init();
        }

        protected void Init()
        {
            GameManager.Entities.Add(this);

            CurrentHealth = MaxHealth;

            UpdateTarget();
        }
        
        private void OnDestroy()
        {
            GameManager.Entities.Remove(this);
        }

        public GameObject UpdateTarget() //Updates entity's target and returns it.
        {
            var go = GameManager.Entities
                .Where(e => e.Team != Team && e.IsAlive && !e.IsInvulnerable)
                .OrderBy(e => Vector2.Distance(e.transform.position, transform.position))
                .FirstOrDefault()
                ?.gameObject; //Finds closest non-dead game entity object on the opposing team

            if (go != null)
            {
                Target = go.transform; //Update the Target variable
                OnTargetChange?.Invoke(); //Invoke the target changed method
            }
            else return null;

            return go; //Returns the game object
        }
        
        #region IDamageable
        public virtual Team Team { get; protected set; }
        public virtual float DamageMultiplier { get; protected set; } = 1f;
        public virtual float HealMultiplier { get; protected set; } = 1f;
        public virtual int MaxHealth { get; protected set; }
        public virtual float CurrentHealth { get; protected set; }
        public virtual bool IsInvulnerable { get; protected set; }
        public bool IsAlive => CurrentHealth > 0;
        public virtual void ApplyDamage(float damage)
        {
            if (damage == 0 || IsInvulnerable) return;

            (damage > 0 ? OnHurt : OnHeal)?.Invoke();

            switch (damage)
            {
                case > 0:
                    CurrentHealth -= damage * DamageMultiplier;
                    break;
                case < 0:
                    CurrentHealth -= damage * HealMultiplier;
                    break;
            }

            OnHealthChanged?.Invoke();

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); //Clamp health
            
            if (CurrentHealth <= 0) Die();
        }
        
        public void SetHealth(int newHealth)
        {
            OnHealthChanged?.Invoke();
            CurrentHealth = newHealth;
        }

        public virtual void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChanged;
        public event IDamageable.HealthAction OnHeal;
        public event IDamageable.HealthAction OnHurt;
        public event IDamageable.HealthAction OnDeath;

        #endregion
    }
}