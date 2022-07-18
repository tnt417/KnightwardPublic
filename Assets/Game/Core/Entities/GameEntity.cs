using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Entities
{
    public abstract class GameEntity : MonoBehaviour, IDamageable
    {
        public delegate void TargetChangeAction();
        public event TargetChangeAction OnTargetChange;
        [NonSerialized] public List<Transform> Targets = new ();
        
        private const float EntityTargetUpdatingRate = 0.1f;
        private float _targetUpdateTimer;
        [SerializeField] private string targetTag;
        [SerializeField] private Team targetTeam;
        [SerializeField] private int maxTargets;
        private void Start()
        {
            Init();
        }

        private void LateUpdate()
        {
            _targetUpdateTimer += Time.deltaTime;
            
            if (_targetUpdateTimer > EntityTargetUpdatingRate)
            {
                UpdateTarget();
                _targetUpdateTimer = 0f;
            }
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

        public List<Transform> UpdateTarget() //Updates entity's target and returns it.
        {
            /* Valid targets match our targetTag variable, match our targetTeam variable, do not target invulnerable or dead things (unless this object is a tower),
             * and only attack things within 10 tiles unless it is the crystal
             * 
             */
            
            var transforms = GameManager.Entities
                .Where(e => (string.IsNullOrEmpty(targetTag) || e.CompareTag(targetTag)) &&
                            e.Team == targetTeam && (this is Tower || !e.IsInvulnerable && e.IsAlive) && e != this
                            && (e is Crystal || Vector2.Distance(e.transform.position, transform.position) < 10f))
                .OrderBy(e => Vector2.Distance(e.transform.position, transform.position))
                .Select(e => e.transform).Take(maxTargets).ToList(); //Finds closest non-dead game entity object on the opposing team
            
            if (transforms.Count == 0) return null;
            
            Targets = transforms;
            OnTargetChange?.Invoke(); //Invoke the target changed method

            return transforms; //Returns the game object
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