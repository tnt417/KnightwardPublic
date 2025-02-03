using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.UI.Tower;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities
{

    public abstract class GameEntity : NetworkBehaviour, IDamageable, IHideable
    {
        public event Action OnTargetChangeOwner;
        public event Action OnTargetChangeGlobal;
        public readonly SyncList<GameEntity> Targets = new();
        //[SerializeReference]public GameEffectList startingEffects = new();

        public const float EntityTargetUpdatingRate = 0.1f;
        private float _targetUpdateTimer;

        public virtual bool CanAttack => (IsAlive || this is Tower);

        //Editor fields
        [Header("Targeting")] [SerializeField] private string targetTag;
        [SerializeField] private Team targetTeam;
        [SerializeField] private int maxTargets;
        [SerializeField] protected StatBonus[] baseStats;
        [SerializeField] public float targetRange = 8f;
        [SerializeField] private bool targetIntangible;
        // 

        //Value is only accurate on the host
        [NonSerialized] public bool VisibleToHost = true;

        public readonly EntityStats Stats = new();

        #region Network

        [Command(requiresAuthority = false)]
        public void CmdSendEffectInfo(string info)
        {
            RpcSendEffectInfo(info);
        }

        [ClientRpc]
        private void RpcSendEffectInfo(string info)
        {
            OnEffectInfoReceive?.Invoke(info);
        }

        public Action<string> OnEffectInfoReceive;
        
        [Command(requiresAuthority = false)]
        public void CmdDamageEntity(float damage, bool isCrit, NetworkIdentity exclude, bool ignoreInvincibility, DamageType dt)
        {
            var successful = true;

            var dmg = this is Player.Player
                ? damage
                : ApplyDamage(damage, out successful,
                    ignoreInvincibility); //Players should have already been damaged on the client
            if (successful) GameManager.Instance.RpcSpawnDmgPopup(transform.position, dmg, isCrit, exclude, dt);
        }

        [Command(requiresAuthority = false)]
        public void CmdRemoveBonusesFromSource(string source)
        {
            Stats.RemoveStatBonuses(source);
        }
        
        [Client]
        protected void SetNetworkHealth(float curHealth, float maxHealth)
        {
            if (!isOwned)
            {
                Debug.LogWarning("SetHealth called by non-owner!");
                return;
            }

            NetworkCurrentHealth = curHealth;
            NetworkMaxHealth = maxHealth;
        }

        public bool EntityOwnership => !(!authority && !isServer || this is Player.Player && !isLocalPlayer);

        [ServerCallback]
        protected void Start()
        {
            if (NetworkServer.active && this is not Player.Player && !authority)
            {
                netIdentity.AssignClientAuthority(NetworkServer.localConnection);
            }
            
            //CmdAddEffect(new CrystalArmorEffect(), this);
        }

        public override void OnStartAuthority()
        {
            Init();
        }
        
        // [Command(requiresAuthority = false)]
        // public void CmdSetHealth(float currentHealth, float maxHealth)
        // {
        //     NetworkCurrentHealth = currentHealth;
        //     NetworkMaxHealth = maxHealth;
        // }

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

            ClientHealthDisparity = 0;
        }

        #endregion
        
        public NetworkIdentity currentParentIdentityLocal;

        public NetworkIdentity CurrentParentIdentity
        {
            get => currentParentIdentityLocal;
            set
            { 
                CmdSetParentIdentity(value);
            }
        }

        public Action<NetworkIdentity, NetworkIdentity> OnParentIdentityChange;

        [Command]
        public void CmdSetParentIdentity(NetworkIdentity roomIdentity)
        {
            if (currentParentIdentityLocal != null)
            {
                var oldRoom = RoomManager.Instance.GetRoomFromID(currentParentIdentityLocal.netId);
                if (oldRoom != null)
                {
                    oldRoom.roomChildObjects.Remove(gameObject);
                }
            }

            var oldId = currentParentIdentityLocal;
            currentParentIdentityLocal = roomIdentity;
            OnParentIdentityChange?.Invoke(oldId, roomIdentity);
            RpcSetParentIdentity(roomIdentity);
            
            if (roomIdentity != null)
            {
                var room = RoomManager.Instance.GetRoomFromID(roomIdentity.netId);
                if (room != null)
                {
                    room.roomChildObjects.Add(gameObject);
                }
            }

            ParentInterestManagement.Instance.ForceRebuild();
        }

        [ClientRpc]
        private void RpcSetParentIdentity(NetworkIdentity identity)
        {
            if (identity == currentParentIdentityLocal)
            {
                return;
            }

            var oldId = currentParentIdentityLocal;
            currentParentIdentityLocal = identity;
            OnParentIdentityChange?.Invoke(oldId, identity);
        }

        #region Attack

        protected virtual float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);
        protected float AttackTimer;
        public Action OnAttack;
        public float NormalizedAttackTime => AttackTimer/AttackTimerMax;

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

        //[Command(requiresAuthority = false)]
        [Client]
        public void SetInvulnerable(bool invuln)
        {
            if(!isOwned)
            {
                Debug.LogWarning("SetInvulnerable called without ownership!");
                return;
            }
            
            invulnerable = invuln;
        }
        
        protected IEnumerator IntangibleForSeconds(float seconds)
        {
            SetInvulnerable(true);
            IsTangible = false;
            yield return new WaitForSeconds(seconds);
            SetInvulnerable(false);
            IsTangible = true;
        }

        protected void Awake()
        {
            GameManager.AddEntity(this);

            OnAttack += () => AttackTimer = 0;
            _effects.OnChange += OnEffectsUpdated;

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

            if(CanAttack) AttackTimer += Time.deltaTime;
            
            if (AttackTimer > AttackTimerMax)
            {
                Attack();
            }

            if (!Stats.ReadOnly && _statsDirty)
            {
                CmdSendStatsToClients(Stats.StatValues.Keys.ToArray(), Stats.StatValues.Values.ToArray());
            }
            
            if (this is Player.Player || !isOwned) return;
            
            _targetUpdateTimer += Time.deltaTime;

            if (_targetUpdateTimer > EntityTargetUpdatingRate)
            {
                if (CurrentParentIdentity == null)
                {
                    UpdateTargets();
                }
                else
                {
                    var room = RoomManager.Instance.GetRoomFromID(CurrentParentIdentity.netId);
                    if(room != null && room.PlayerCount > 0) UpdateTargets();
                }

                _targetUpdateTimer -= EntityTargetUpdatingRate;
            }
        }

        protected void Init()
        {
            if (!EntityOwnership) return;
            
            Stats.ReadOnly = false;

            Stats.OnStatChanged += (_) => FlagStatsAsDirty(true) ;
            Stats.OnStatChanged += (stat) =>
            {
                if (stat != Stat.Health) return;
                SetNetworkHealth(CurrentHealth, MaxHealth);
                //CmdSetHealth(CurrentHealth, MaxHealth);
            };
            
            foreach (var sb in baseStats)
            {
                Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, "GameEntity");
            }

            FlagStatsAsDirty(true);

            UpdateTargets();
            
            OnHealthChangedOwner += (value) =>
            {
                SetNetworkHealth(value, MaxHealth);
                //CmdSetHealth(value, MaxHealth);
            };
            OnHealthChangedOwner?.Invoke(MaxHealth);

            /*if (startingEffects is {gameEffects: { }})
            {
                foreach (var se in startingEffects.gameEffects)
                {
                    if(se != null) CmdAddEffect(se, this);
                }
            }*/
        }

        public Action OnStart;

        public Action<float, GameEntity, bool, DamageType>
            OnDamageOther; //TODO: Damage types: Contact, Projectile, DoT, AoE, etc. (Use to better control PoisonInflictEffect)

        private bool _statsDirty = false;
        
        private void FlagStatsAsDirty(bool dirty)
        {
            _statsDirty = true;
        }
        
        private void UpdateStat(Stat stat)
        {
            //if (Stats.ReadOnly) return;
            CmdSendStatsToClients(Stats.StatValues.Keys.ToArray(), Stats.StatValues.Values.ToArray());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            CmdRequestUpdateStats();
            OnStart?.Invoke();
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
        private void CmdSendStatsToClients(Stat[] keys, float[] values)
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

        public void LocalHurt(float damage, bool isCrit, DamageType dt)
        {
            if (!IsInvulnerable)
            {
                ObjectSpawner.SpawnDmgPopup(transform.position, Stats.ModifyIncomingDamage(damage), isCrit, dt);
                OnLocalHurt?.Invoke();
            }
        }

        //[Command(requiresAuthority = false)]
        [Client]
        public void UpdateTargets() //Updates entity's target and returns it.
        {
            /* Valid targets match our targetTag variable, match our targetTeam variable, do not target invulnerable or dead things (unless this object is a tower),
             * and only attack things within 10 tiles unless it is the crystal
             * 
             */

            if (!isOwned)
            {
                Debug.LogWarning("Update targets called from non-owner!");
                return;
            }

            if (this is Player.Player) return;
            
            var range = targetRange;

            var myPos = transform.position;

            var entitiesSet = GameManager.EntitiesReadonly;

            SortedSet<KeyValuePair<float, GameEntity>> distances = new(new ByClosest());

            foreach (var ge in entitiesSet)
            {
                if (ge == null || ge.Team != targetTeam || (!ge.IsTangible && !targetIntangible) || ge == this ||
                    (!string.IsNullOrEmpty(targetTag) && !ge.CompareTag(targetTag)) ||
                    ge.CurrentParentIdentity != CurrentParentIdentity) continue;
                    
                var dist = Vector2.Distance(ge.transform.position, myPos);
                
                if(!(dist < (ge is Crystal && this is not Tower ? 200f : range))) continue;
                
                distances.Add(new KeyValuePair<float, GameEntity>(dist, ge));
            }

            Targets.Clear();

            var index = 0;
            
            ////Debug.Log("B");

            foreach (var ge in distances)
            {
                index++;
                Targets.Add(ge.Value);

                if (index >= maxTargets) break;
            }
            
            //Profiler.EndSample();

            OnTargetChangeOwner?.Invoke(); //Invoke the target changed method
        }

        #region Effects

        private readonly SyncList<GameEffect> _effects = new();
        public List<GameEffect> EffectsReadonly => _effects.ToList();
        
        private void OnEffectsUpdated(SyncList<GameEffect>.Operation op, int index, GameEffect effect)
        {
            switch (op)
            {
                case SyncList<GameEffect>.Operation.OP_ADD:
                    effect.Entity = this;
                    if(isServer) effect.OnAddServer();
                    if (EntityOwnership) effect.OnAddOwner();
                    effect.OnAddClient();
                    break;
                case SyncList<GameEffect>.Operation.OP_REMOVEAT:
                    if (GameEffect.GameEffectIdentifiers.ContainsKey(effect.EffectIdentifier))
                        GameEffect.GameEffectIdentifiers.Remove(effect.EffectIdentifier);
                    if(isServer) effect.OnRemoveServer();
                    if (EntityOwnership) effect.OnRemoveOwner();
                    effect.OnRemoveClient();
                    break;
            }
        }

        //[Command(requiresAuthority = false)]
        public void AddEffect(GameEffect effect, GameEntity source)
        {
            if (!EntityOwnership)
            {
                Debug.LogWarning("Add effect called from non-owner! Calling command instead");
                CmdAddEffect(effect, source);
                return;
            }            
            
            effect.Entity = this;
            effect.Source = source;

            _effects.Add(effect);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdAddEffect(GameEffect effect, GameEntity source)
        {
            AddEffect(effect, source);
        }
        
        public void RemoveEffect(GameEffect effect)
        {
            if (!EntityOwnership)
            {
                Debug.LogWarning("Remove effect called from non-owner!");
                return;
            }
            
            if (!_effects.Contains(effect)) return;

            _effects.Remove(effect);
        }

        public bool HasEffect(GameEffect effect) => _effects.Contains(effect);

        public bool HasEffectOfType<T>() => _effects.Any(ge => ge is T);

        public void RemoveEffectsOfType<T>() where T : GameEffect
        {
            if (!EntityOwnership) return;

            var removals = _effects.Where(ge => ge.GetType() == typeof(T)).ToList();

            foreach (var ge in removals)
            {
                RemoveEffect(ge);
            }
        }

        public void RemoveEffectsOfType(string type)
        {
            if (!EntityOwnership) return;

            var removals = _effects.Where(ge => ge.GetType().Name == type).ToList();

            foreach (var ge in removals)
            {
                RemoveEffect(ge);
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
        public virtual float CurrentHealth => NetworkCurrentHealth;
        [SyncVar] public bool invulnerable = false;
        public virtual bool IsInvulnerable => invulnerable;
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

            var newHp = CurrentHealth;
            
            switch (modifiedDamage)
            {
                case > 0:
                    newHp -= modifiedDamage * DamageMultiplier;
                    break;
                case < 0:
                    newHp -= modifiedDamage * HealMultiplier;
                    break;
            }

            newHp = Mathf.Clamp(newHp, 0, MaxHealth); //Clamp health
            
            OnHealthChangedOwner?.Invoke(newHp);

            if (newHp <= 0 && !IsInvulnerable) Die();

            return modifiedDamage;
        }

        public void FullHeal()
        {
            if (!EntityOwnership) return;

            OnHealthChangedOwner?.Invoke(MaxHealth);
        }
        
        public void SetHealth(float newHealth)
        {
            if (!EntityOwnership) return;

            OnHealthChangedOwner?.Invoke(newHealth);
        }

        public virtual void Die()
        {
            if (!EntityOwnership) return;

            OnDeathOwner?.Invoke(0);

            if (this is not Player.Player) NetworkServer.Destroy(gameObject);
        }

        protected void TriggerDeathOwner()
        {
            OnDeathOwner?.Invoke(0);
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

    public enum DamageType
    {
        Default, DoT, AoE, Absolute, Contact, Projectile, Heal
    }

    public class ByClosest : IComparer<KeyValuePair<float, GameEntity>>
    {
        public int Compare(KeyValuePair<float, GameEntity> x, KeyValuePair<float, GameEntity> y)
        {
            var (xK, _) = x;
            var (yK, _) = y;
            
            return xK > yK ? 1 : -1;
        }
    }
}