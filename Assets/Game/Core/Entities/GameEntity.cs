using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev.Game.Core.Entities
{
    public abstract class GameEntity : NetworkBehaviour, IDamageable
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

        //Value is only accurate on the host
        public bool visibleToHost = true;

        public readonly EntityStats Stats = new ();

        #region Network

        protected bool EntityOwnership => !(!hasAuthority && !isServer || this is Player.Player && !isLocalPlayer);
        
        [Command(requiresAuthority = false)]
        public void CmdSetHealth(float currentHealth, float maxHealth)
        {
            networkCurrentHealth = currentHealth;
            networkMaxHealth = maxHealth;
        }

        [SyncVar] public float networkCurrentHealth;
        [SyncVar] public float networkMaxHealth;
        
        #endregion

        [SyncVar] public NetworkIdentity currentParentIdentity;
        
        [Command(requiresAuthority = false)]
        public void CmdSetParentIdentity(NetworkIdentity roomIdentity)
        {
            currentParentIdentity = roomIdentity;
            if(this is Player.Player) FindObjectOfType<ParentInterestManagement>().ForceRebuild();
        }

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
        protected void Awake()
        {
            if(isLocalPlayer) Player.Player.OnLocalPlayerCreated += Init;

            OnAttack += () => _attackTimer = 0;
        }

        protected void Update()
        {
            if (!EntityOwnership) return;
            
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

        protected void Init()
        {
            GameManager.AddEntity(this);

            if (!EntityOwnership) return;

            CustomNetworkManager.OnAllPlayersSpawned += UpdateStats;
            
            CurrentHealth = MaxHealth;
            OnHealthChanged += (float value) => CmdSetHealth(CurrentHealth, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);

            Stats.ReadOnly = false;
            Stats.OnStatsChanged += UpdateStats;

            UpdateStats();
            
            UpdateTarget();
        }

        private void UpdateStats()
        {
            CmdUpdateStats(Stats.StatValues.Keys.ToArray(), Stats.StatValues.Values.ToArray());
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateStats(Stat[] keys, float[] values)
        {
            RpcUpdateStats(keys, values);
        }

        [ClientRpc]
        private void RpcUpdateStats(Stat[] keys, float[] values)
        {
            if(Stats.ReadOnly) Stats.ReplaceStatValueDictionary(keys, values);
        }

        private void OnDestroy()
        {
            GameManager.RemoveEntity(this);
        }

        public void UpdateTarget() //Updates entity's target and returns it.
        {
            if (!EntityOwnership) return;

            /* Valid targets match our targetTag variable, match our targetTeam variable, do not target invulnerable or dead things (unless this object is a tower),
             * and only attack things within 10 tiles unless it is the crystal
             * 
             */

            var range = 10f;
            if (this is Tower t) range = t.targetRadius;
            
            var transforms = GameManager.EntitiesReadonly
                .Where(e => e != null 
                            && e.currentParentIdentity == currentParentIdentity //If both have the same parent netId
                            && (string.IsNullOrEmpty(targetTag) || e.CompareTag(targetTag)) //Target things with our target tag if it is set
                            && e.Team == targetTeam //Target things of our target team
                            && (this is Tower || !e.IsInvulnerable && e.IsAlive) //Don't target invulnerable things, unless we are a tower (meant for tesla tower)
                            && e != this //Don't target self
                            && (e is Crystal && Vector2.Distance(e.transform.position, transform.position) < 200f 
                                || Vector2.Distance(e.transform.position, transform.position) < range)) //Distance check
                .OrderBy(e => Vector2.Distance(e.transform.position, transform.position))
                .Select(e => e.transform).Take(maxTargets).ToList(); //Find closest non-dead game entity object on the opposing team

            Targets = transforms;
            OnTargetChange?.Invoke(); //Invoke the target changed method
        }
        
        #region Effects

        private readonly List<GameEffect> _effects = new();

        public void AddEffect(GameEffect effect, GameEntity source)
        {
            if (!EntityOwnership) return;
            
            _effects.Add(effect);
            effect.Entity = this;
            effect.OnAdd(source);
        }
        
        public void RemoveEffect(GameEffect effect)
        {
            if (!EntityOwnership) return;
            
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
        public bool IsAlive => networkCurrentHealth > 0;
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

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); //Clamp health
            
            OnHealthChanged?.Invoke(modifiedDamage);

            if (CurrentHealth <= 0) Die();

            return modifiedDamage;
        }
        
        public void SetHealth(float newHealth)
        {
            if (!EntityOwnership) return;
            
            OnHealthChanged?.Invoke(newHealth);
            CurrentHealth = newHealth;
        }

        public virtual void Die()
        {
            if (!EntityOwnership) return;
            
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
