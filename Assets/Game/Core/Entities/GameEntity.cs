using System;
using System.Collections;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities
{

    public abstract class GameEntity : NetworkBehaviour, IDamageable, IHideable
    {
        public event Action OnTargetChangeOwner;
        public SyncList<GameEntity> Targets = new();

        public const float EntityTargetUpdatingRate = 0.1f;
        private float _targetUpdateTimer;

        protected virtual bool CanAttack => IsAlive || this is Tower;

        //Editor fields
        [Header("Targeting")] [SerializeField] private string targetTag;
        [SerializeField] private Team targetTeam;
        [SerializeField] private int maxTargets;
        [SerializeField] protected StatBonus[] baseStats;

        [SerializeField] private bool targetIntangible;
        // 

        //Value is only accurate on the host
        [NonSerialized] public bool VisibleToHost = true;

        public readonly EntityStats Stats = new();

        #region Network

        [Command(requiresAuthority = false)]
        public void CmdDamageEntity(float damage, bool isCrit, NetworkIdentity exclude, bool ignoreInvincibility)
        {
            /*var entity = entityObject.GetComponent<GameEntity>();

            if (entity == null)
            {
                Debug.LogWarning($"Net object {entityObject.gameObject.name} is not an entity!");
                return;
            }*/

            var successful = true;

            var dmg = this is Player.Player
                ? damage
                : ApplyDamage(damage, out successful,
                    ignoreInvincibility); //Players should have already been damaged on the client
            if (successful) GameManager.Instance.RpcSpawnDmgPopup(transform.position, dmg, isCrit, exclude);
        }

        public bool EntityOwnership => !(!hasAuthority && !isServer || this is Player.Player && !isLocalPlayer);

        [Command(requiresAuthority = false)]
        public void CmdSetHealth(float currentHealth, float maxHealth)
        {
            NetworkCurrentHealth = currentHealth;
            NetworkMaxHealth = maxHealth;
        }

        public void ApplyKnockbackGlobal(Vector2 force)
        {
            if (!NetworkClient.active) return;
            GetComponent<Rigidbody2D>()?.AddForce(force);
            CmdApplyKnockback(force);
        }

        [Command(requiresAuthority = false)]
        private void CmdApplyKnockback(Vector2 force)
        {
            GetComponent<Rigidbody2D>()?.AddForce(force);
        }

        [NonSerialized] public float ClientHealthDisparity;

        [SyncVar(hook = nameof(CurrentHealthHook))] [NonSerialized]
        public float NetworkCurrentHealth;

        [SyncVar] [NonSerialized] public float NetworkMaxHealth;

        private void CurrentHealthHook(float oldHealth, float newHealth)
        {
            if (ClientHealthDisparity == 0 || isServer || this is Player.Player) return;

            if (oldHealth == 0) ClientHealthDisparity = 0;
            else
            {
                ClientHealthDisparity -= newHealth - oldHealth;
            }
        }

        #endregion

        [SyncVar(hook = nameof(ParentIdentityHook))]
        private NetworkIdentity _currentParentIdentity;

        public NetworkIdentity CurrentParentIdentity
        {
            get => _currentParentIdentity;
            set
            {
                _currentParentIdentity = CurrentParentIdentity;
                CmdSetParentIdentity(value);
            }
        }

        public Action<NetworkIdentity> OnParentIdentityChange;

        protected virtual void ParentIdentityHook(NetworkIdentity oldIdentity, NetworkIdentity newIdentity)
        {
            OnParentIdentityChange?.Invoke(newIdentity);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetParentIdentity(NetworkIdentity roomIdentity)
        {
            if (_currentParentIdentity != null)
            {
                var oldRoom = _currentParentIdentity.GetComponent<Room>();
                if (oldRoom != null)
                {
                    oldRoom.roomChildObjects.Remove(gameObject);
                }
            }

            _currentParentIdentity = roomIdentity;

            if (roomIdentity != null)
            {
                var room = roomIdentity.GetComponent<Room>();
                if (room != null)
                {
                    room.roomChildObjects.Add(gameObject);
                }
            }

            ParentInterestManagement.Instance.ForceRebuild();
        }

        #region Attack

        protected virtual float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);
        private float _attackTimer;
        public Action OnAttack;

        //Invokes the attack event
        public void Attack() //Called in animator events on some entities
        {
            if (!EntityOwnership) return;

            if (CanAttack)
            {
                OnAttack.Invoke();
            }
        }

        #endregion

        protected IEnumerator IntangibleForSeconds(float seconds)
        {
            IsInvulnerable = true;
            IsTangible = false;
            yield return new WaitForSeconds(seconds);
            IsInvulnerable = false;
            IsTangible = true;
        }

        protected void Awake()
        {
            GameManager.AddEntity(this);

            OnAttack += () => _attackTimer = 0;
            _effects.Callback += OnEffectsUpdated;

            _targetUpdateTimer = Random.Range(0, EntityTargetUpdatingRate);
        }

        protected void Update()
        {
            foreach (var effect in _effects.ToArray())
            {
                if (isServer) effect.OnUpdateServer();
                if (isClient) effect.OnUpdateClient();
            }

            if (!EntityOwnership) return;

            foreach (var effect in _effects.ToArray())
            {
                if (effect == null)
                {
                    _effects.Remove(effect);
                    continue;
                }
                
                effect.OnUpdateOwner();
            }

            if (IsAlive)
            {
                var hpRegen = Stats.GetStat(Stat.HpRegen) * Time.deltaTime; //Regen 1% of hp per second
                ApplyDamage(-hpRegen, out var success, true); //Regen health by HpRegen per second
            }

            _attackTimer += Time.deltaTime;
            
            if (_attackTimer > AttackTimerMax)
            {
                Attack();
            }
            
            if (this is Player.Player) return;
            
            _targetUpdateTimer += Time.deltaTime;

            if (_targetUpdateTimer > EntityTargetUpdatingRate)
            {
                if (CurrentParentIdentity == null)
                {
                    CmdUpdateTarget();
                }
                else
                {
                    var room = CurrentParentIdentity.GetComponent<Room>();
                    if(room.PlayerCount > 0) CmdUpdateTarget();
                }
                
                _targetUpdateTimer -= EntityTargetUpdatingRate;
            }
        }

        protected void Init()
        {
            if (!EntityOwnership) return;

            CustomNetworkManager.OnAllPlayersSpawned += UpdateStats;

            Stats.ReadOnly = false;

            foreach (var sb in baseStats)
            {
                Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, "GameEntity");
            }

            Stats.OnStatsChanged += UpdateStats;
            Stats.OnStatsChanged += () => CmdSetHealth(CurrentHealth, MaxHealth);

            UpdateStats();

            CmdUpdateTarget();
            
            CurrentHealth = MaxHealth;
            OnHealthChangedOwner += (float value) => CmdSetHealth(CurrentHealth, MaxHealth);
            OnHealthChangedOwner?.Invoke(CurrentHealth);
        }

        public Action<float, GameEntity, bool>
            OnDamageOther; //TODO: Damage types: Contact, Projectile, DoT, AoE, etc. (Use to better control PoisonInflictEffect)

        private void UpdateStats()
        {
            if (!NetworkClient.active) return;
            CmdUpdateStats(Stats.StatValues.Keys.ToArray(), Stats.StatValues.Values.ToArray());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            CmdRequestUpdateStats();
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestUpdateStats(NetworkConnectionToClient sender = null)
        {
            TargetUpdateStats(sender, Stats.StatValues.Keys.ToArray(), Stats.StatValues.Values.ToArray());
        }

        [TargetRpc]
        private void TargetUpdateStats(NetworkConnection target, Stat[] keys, float[] values)
        {
            if (Stats.ReadOnly) Stats.ReplaceStatValueDictionary(keys, values);
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateStats(Stat[] keys, float[] values)
        {
            RpcUpdateStats(keys, values);
        }

        [ClientRpc]
        private void RpcUpdateStats(Stat[] keys, float[] values)
        {
            if (Stats.ReadOnly) Stats.ReplaceStatValueDictionary(keys, values);
        }

        private void OnDestroy()
        {
            GameManager.RemoveEntity(this);
        }

        public Action OnLocalHurt;

        public void LocalHurt(float damage, bool isCrit)
        {
            if (!IsInvulnerable)
                ObjectSpawner.SpawnDmgPopup(transform.position, Stats.ModifyIncomingDamage(damage), isCrit);
            OnLocalHurt?.Invoke();
        }

        [Command(requiresAuthority = false)]
        public void CmdUpdateTarget() //Updates entity's target and returns it.
        {
            if (!EntityOwnership) return;

            /* Valid targets match our targetTag variable, match our targetTeam variable, do not target invulnerable or dead things (unless this object is a tower),
             * and only attack things within 10 tiles unless it is the crystal
             * 
             */

            var range = 10f;
            if (this is Tower t) range = t.targetRadius;

            var myPos = transform.position;

            var entities = GameManager.EntitiesReadonly
                .Where(e => e != null
                            && e.CurrentParentIdentity == CurrentParentIdentity //If both have the same parent netId
                            && (string.IsNullOrEmpty(targetTag) || e.CompareTag(targetTag)) //Target things with our target tag if it is set
                            && e.Team == targetTeam //Target things of our target team
                            //&& (this is Tower || !e.IsInvulnerable && e.IsAlive) //Don't target invulnerable things, unless we are a tower (meant for tesla tower)
                            && (e.IsTangible || targetIntangible) //Don't target intangible, unless we target intangible
                            && e != this //Don't target self
                            && Vector2.Distance(e.transform.position, myPos) < (e is Crystal && this is not Tower ? 200f : range)) //Distance check
                .OrderBy(e => Vector2.Distance(e.transform.position, myPos))
                .Take(maxTargets); //Find closest non-dead game entity object on the opposing team

            Targets.Clear();
            //Targets.AddRange(entities);

            foreach (var ge in entities)
            {
                Targets.Add(ge);
            }
            
            // var targetList = Targets.ToList();
            //
            // foreach (var ge in targetList.Where(ge => !entities.Contains(ge)))
            // {
            //     Targets.Remove(ge);
            // }
            //
            // foreach (var ge in entities)
            // {
            //     if (targetList.Contains(ge)) continue;
            //
            //     Targets.Add(ge);
            // }

            OnTargetChangeOwner?.Invoke(); //Invoke the target changed method
        }

        #region Effects

        private SyncList<GameEffect> _effects = new();

        private void OnEffectsUpdated(SyncList<GameEffect>.Operation op, int index, GameEffect oldEffect,
            GameEffect newEffect)
        {
            switch (op)
            {
                case SyncList<GameEffect>.Operation.OP_ADD:
                    newEffect.Entity = this;
                    if (EntityOwnership) newEffect.OnAddOwner();
                    newEffect.OnAddClient();
                    break;
                case SyncList<GameEffect>.Operation.OP_REMOVEAT:
                    if (EntityOwnership) oldEffect.OnRemoveOwner();
                    oldEffect.OnRemoveClient();
                    break;
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdAddEffect(GameEffect effect, GameEntity source)
        {
            effect.Entity = this;
            effect.Source = this;

            _effects.Add(effect);
            effect.OnAddServer();
        }

        [Command(requiresAuthority = false)]
        public void CmdRemoveEffect(GameEffect effect)
        {
            if (!_effects.Contains(effect)) return;

            _effects.Remove(effect);

            if (GameEffect.GameEffectIdentifiers.ContainsKey(effect.EffectIdentifier))
                GameEffect.GameEffectIdentifiers.Remove(effect.EffectIdentifier);

            effect.OnRemoveServer();
        }

        public bool HasEffect(GameEffect effect) => _effects.Contains(effect);

        public bool HasEffectOfType<T>() => _effects.Any(ge => ge is T);

        public void RemoveEffectsOfType<T>() where T : GameEffect
        {
            if (!EntityOwnership) return;

            var removals = _effects.Where(ge => ge.GetType() == typeof(T)).ToList();

            foreach (var ge in removals)
            {
                CmdRemoveEffect(ge);
            }
        }

        public void RemoveEffectsOfType(string type)
        {
            if (!EntityOwnership) return;

            var removals = _effects.Where(ge => ge.GetType().Name == type).ToList();

            foreach (var ge in removals)
            {
                CmdRemoveEffect(ge);
            }
        }

        #endregion

        #region IDamageable

        [field:SerializeField] public virtual Team Team { get; protected set; } = default;

        public Action<Team> OnTeamChange;

        public void
            SetTeam(Team newTeam,
                Team newTargetTeam) //Changes our team, invoking an event. WILL ONLY CONVERT ATTACKS' TEAMS THAT ARE ON THE SAME TEAM
        {
            if (newTeam == Team) return;

            OnTeamChange?.Invoke(newTeam);

            targetTeam = newTargetTeam;
            Team = newTeam;
        }

        public virtual float DamageMultiplier { get; protected set; } = 1f;
        public virtual float HealMultiplier { get; set; } = 1f;
        public virtual int MaxHealth => (int) Stats.GetStat(Stat.Health);
        public virtual float CurrentHealth { get; protected set; }
        public virtual bool IsInvulnerable { get; set; }
        public virtual bool IsTangible { get; set; } = true;
        public bool IsAlive => NetworkCurrentHealth > 0;

        public virtual float ApplyDamage(float damage, out bool successful, bool ignoreInvincibility = false)
        {
            successful = !IsInvulnerable || ignoreInvincibility || damage < 0;

            if (damage == 0) return 0;

            if (damage > 0 && IsInvulnerable)
            {
                OnTryHurtInvulnerableOwner?.Invoke(damage);
                return 0;
            }

            if (damage > 0 && Stats.DodgeSuccessful) return 0; //Don't apply damage is dodge rolls successful.
            var modifiedDamage = damage >= 0
                ? Mathf.Clamp(Stats.ModifyIncomingDamage(damage), 0, Mathf.Infinity)
                : damage;

            (modifiedDamage > 0 ? OnHurtOwner : OnHealOwner)?.Invoke(modifiedDamage);

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

            OnHealthChangedOwner?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0 && !IsInvulnerable) Die();

            return modifiedDamage;
        }

        public void FullHeal()
        {
            if (!EntityOwnership) return;

            CurrentHealth = MaxHealth;
            OnHealthChangedOwner?.Invoke(CurrentHealth);
        }
        
        public void SetHealth(float newHealth)
        {
            if (!EntityOwnership) return;

            CurrentHealth = newHealth;
            OnHealthChangedOwner?.Invoke(newHealth);
        }

        public virtual void Die()
        {
            if (!EntityOwnership) return;

            OnDeathOwner?.Invoke(0);

            if (this is not Player.Player) NetworkServer.Destroy(gameObject);
        }

        public event IDamageable.HealthAction OnTryHurtInvulnerableOwner;
        public event IDamageable.HealthAction OnHealthChangedOwner;
        public event IDamageable.HealthAction OnHealOwner;
        public event IDamageable.HealthAction OnHurtOwner;
        public event IDamageable.HealthAction OnDeathOwner;

        #endregion

        // public Action OnDeathBroadcast;
        //
        // [Command(requiresAuthority = false)]
        // private void CmdBroadcastDeath()
        // {
        //     RpcBroadcastDeath();
        // }
        //
        // [ClientRpc]
        // private void RpcBroadcastDeath()
        // {
        //     OnDeathBroadcast?.Invoke();
        // }
    }
}