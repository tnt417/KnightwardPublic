using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
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

        protected virtual bool CanAttack => IsAlive;
        
        //Editor fields
        [Header("Targeting")]
        [SerializeField] private string targetTag;
        [SerializeField] private Team targetTeam;
        [SerializeField] private int maxTargets;
        // 

        public readonly EntityStats Stats = new ();

        
        #region Attack
        protected virtual float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);
        private float _attackTimer;
        
        public delegate void AttackAction();
        public event AttackAction OnAttack;
        
        //Invokes the attack event
        public void Attack() //Called in animator events on some entities
        {
            if(CanAttack) OnAttack?.Invoke();
        }
        #endregion

        private delegate void TargetAction();
        protected void Start()
        {
            Init();

            OnAttack += () => _attackTimer = 0;
        }

        protected void Update()
        {
            _targetUpdateTimer += Time.deltaTime;
            _attackTimer += Time.deltaTime;
            
            if (_targetUpdateTimer > EntityTargetUpdatingRate)
            {
                UpdateTarget();
                _targetUpdateTimer = 0f;
            }

            if (_attackTimer > AttackTimerMax)
            {
                Attack();
            }

            _effects.RemoveAll(e => e == null);
            
            foreach (var effect in _effects.ToArray())
            {
                effect.OnUpdate();
            }
            
            if (IsAlive)
            {
                var hpRegen = Stats.GetStat(Stat.HpRegen) * Time.deltaTime; //Regen 1% of hp per second
                ApplyDamage(-hpRegen); //Regen health by HpRegen per second
            }
        }

        private void Init()
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

            var range = 10f;
            if (this is Tower t) range = t.targetRadius;
            
            var transforms = GameManager.Entities
                .Where(e => (string.IsNullOrEmpty(targetTag) || e.CompareTag(targetTag)) &&
                            e.Team == targetTeam && (this is Tower || !e.IsInvulnerable && e.IsAlive) && e != this
                            && ((e is Crystal && Vector2.Distance(e.transform.position, transform.position) < 200f)|| Vector2.Distance(e.transform.position, transform.position) < range))
                .OrderBy(e => Vector2.Distance(e.transform.position, transform.position))
                .Select(e => e.transform).Take(maxTargets).ToList(); //Finds closest non-dead game entity object on the opposing team
            
            if (transforms.Count == 0) return null;
            
            Targets = transforms;
            OnTargetChange?.Invoke(); //Invoke the target changed method

            return transforms; //Returns the game object
        }
        
        #region Effects

        private readonly List<GameEffect> _effects = new();

        public void AddEffect(GameEffect effect, GameEntity source)
        {
            Debug.Log("Added effect: " + nameof(effect));
            _effects.Add(effect);
            effect.Entity = this;
            effect.OnAdd(source);
        }
        
        public void RemoveEffect(GameEffect effect)
        {
            _effects.Remove(effect);
            effect.OnRemove();
        }
        
        #endregion
        
        #region IDamageable
        public virtual Team Team { get; protected set; } = default;
        public virtual float DamageMultiplier { get; protected set; } = 1f;
        public virtual float HealMultiplier { get; protected set; } = 1f;
        public virtual int MaxHealth => (int)Stats.GetStat(Stat.Health);
        public virtual float CurrentHealth { get; protected set; }
        public virtual bool IsInvulnerable { get; protected set; }
        public bool IsAlive => CurrentHealth > 0;
        public virtual float ApplyDamage(float damage)
        {
            if (damage == 0 || IsInvulnerable) return 0;

            if (Stats.DodgeSuccessful) return 0; //Don't apply damage is dodge rolls successful.
            var modifiedDamage = damage >= 0
                ? Mathf.Clamp(Stats.ModifyIncomingDamage(damage), 0, Mathf.Infinity)
                : damage;
            
            (modifiedDamage > 0 ? OnHurt : OnHeal)?.Invoke(modifiedDamage);

            switch (modifiedDamage)
            {
                case > 0:
                    CurrentHealth -= modifiedDamage * DamageMultiplier;
                    break;
                case < 0:
                    CurrentHealth -= modifiedDamage * HealMultiplier;
                    break;
            }

            OnHealthChanged?.Invoke(modifiedDamage);

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); //Clamp health
            
            if (CurrentHealth <= 0) Die();

            return modifiedDamage;
        }
        
        public void SetHealth(int newHealth)
        {
            OnHealthChanged?.Invoke(newHealth);
            CurrentHealth = newHealth;
        }

        public virtual void Die()
        {
            OnDeath?.Invoke(0);
            Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnHealthChanged;
        public event IDamageable.HealthAction OnHeal;
        public event IDamageable.HealthAction OnHurt;
        public event IDamageable.HealthAction OnDeath;

        #endregion
    }
}