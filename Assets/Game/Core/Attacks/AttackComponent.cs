using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Attacks
{
    public enum Team
    {
        Enemy,
        Player,
        Environment
    }

    public class AttackComponent : MonoBehaviour
    {
        public string identifier;

        private static int _nextIdentifierSuffix;
        
        public static string GetUniqueIdentifier(GameEntity owner)
        {
            _nextIdentifierSuffix++;
            return "CONN" + NetworkClient.localPlayer.netId + "_OWNER" + owner.netIdentity.netId + "_" + _nextIdentifierSuffix;
        }
        
        
        #region Variables

        private const float KnockbackForce = 1000f; //A multiplier so knockback values can be for example 20 instead of 20000

        #region Inspector Variables

        [Tooltip("The base damage of the attack. May be altered based on the owner entity's stats.")] [SerializeField]
        public float damage;

        private float Damage => _owner != null ? _owner.Stats.GetStat(Stat.Damage) : damage;

        [Tooltip("Multiplies the damage dealt to things.")] [SerializeField]
        public float damageMultiplier;

        [Tooltip("Multiplies the knockback applied to things.")] [SerializeField]
        public float knockbackMultiplier = 1f;
        
        [Tooltip("Cooldown in seconds between applying damage to individual GameObjects.")] [SerializeField]
        public float damageCooldown;

        [Tooltip("The attack's team. Damage won't be applied to objects on the same team.")] [SerializeField]
        public Team team;

        [Tooltip("Should the object destroy when it deals damage?")] [SerializeField]
        public bool destroyOnApply;

        [Tooltip("Should the object destroy when its collider collides?")] [SerializeField]
        public bool destroyOnAnyCollide;

        [Tooltip("Buffs inflicted to GameEntities upon applying damage")] [SerializeField]
        private List<StatBonus> inflictBuffs = new();

        [Tooltip("Effects inflicted to GameEntities upon applying damage")] [SerializeField]
        private List<string> inflictEffects = new();

        [Tooltip("The Rigidbody used to check collisions for attack logic.")]
        public Rigidbody2D rb2d;

        #endregion

        private bool IsCriticalHit =>
            _owner != null && _owner.Stats.CritSuccessful; //Rolls for critical hit using owner entity's bool.

        [NonSerialized] private GameEntity
            _owner; //The owner of the object. Set upon creation. Used to access stats and to ensure that attacks don't harm the owner.

        private Dictionary<GameObject, float>
            _hitCooldowns = new(); //Holds timers until each object is able to be hit again.

        #endregion

        private Vector2 _velocity;
        private Vector2 _oneFrameAgo;
 
        private void FixedUpdate () {
            _velocity = (Vector2)transform.position - _oneFrameAgo;
            _oneFrameAgo = transform.position;
        }
        
        private void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d == null) rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.isKinematic = true; //Add a RigidBody if there isn't one

            if (_owner == null)
                _owner = GetComponent<GameEntity>(); //Owner is our own GameEntity if it hasn't been set yet.
        }

        private void Update()
        {
            if (team == Team.Enemy && Damage > 0)
            {
                damageMultiplier =
                    1 + Mathf.Log10(GameManager.EnemyDifficultyScale) * 0.5f; //Enemy damage scales as time passes.
            }

            //Tick hit cooldown timers
            Dictionary<GameObject, float> newHitCooldowns = new();

            foreach (var go in _hitCooldowns.Keys)
            {
                if (go == null || !_hitCooldowns.ContainsKey(go)) continue;
                var newTime = _hitCooldowns[go] - Time.deltaTime;
                if (newTime > 0) newHitCooldowns.Add(go, newTime);
                else
                {
                    _hitCooldowns = newHitCooldowns;
                    CheckCollision(go);
                }
            }

            _hitCooldowns = newHitCooldowns;
            //
        }

        private bool _quitting;
        
        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        private void OnDestroy()
        {
            if (_quitting) return;
            
            GameManager.Instance.projectiles.Remove(gameObject);
            GameManager.Instance.CmdDestroyProjectile(identifier);
        }

        #region DamageHandling

        private void
            CheckCollision(
                GameObject go) //Checks if we are colliding with an object. Called when their hit cooldown timer is up.
        {
            if (go == null) return;
            var otherTrigger = go.GetComponents<Collider2D>().FirstOrDefault(c2d => c2d.isTrigger);
            if (rb2d != null && rb2d.IsTouching(otherTrigger)) //If colliding, try damaging it.
                TryDamage(otherTrigger);
        }

        private void TryDamage(Collider2D other)
        {
            var netId = other.GetComponent<NetworkIdentity>();
            var entity = other.GetComponent<GameEntity>();

            //Don't want to return in cases where the thing being hit is local player, so hit detection doesn't have latency with the player.
            if (_owner != null && !_owner.hasAuthority && (entity == null || entity is not Player))
                return; //Only call damage code on attacks that are owned by our client.

            var damageable = other.GetComponent<IDamageable>();
            //
            if (damageable == null || damageable.Team == team || damageable.IsInvulnerable ||
                _hitCooldowns.ContainsKey(other.gameObject) ||
                !other.isTrigger) return; //Check if valid thing to hit

            var crit = IsCriticalHit;
            
            float modifiedDamage = (int) (Damage * damageMultiplier *
                                          (crit
                                              ? 2
                                              : 1));

            if (entity is Player && entity != Player.LocalInstance) return;
            
            if (netId != null)
            {
                var dmg = netId == NetworkClient.localPlayer
                    ? entity.ApplyDamage(modifiedDamage)
                    : modifiedDamage; //Do damage before command if hitting player
                if(entity is not Player) entity.clientHealthDisparity -= dmg;
                entity.LocalHurt(dmg, crit);
                GameManager.Instance.CmdDamageEntity(netId, dmg, crit, NetworkClient.localPlayer);
                
                if (inflictEffects != null) //Inflict effects...
                    foreach (var e in inflictEffects)
                    {
                        GameManager.Instance.CmdApplyEffect(entity, _owner, e);
                    }
            }
            else
            {
                var damageDealt =
                    damageable.ApplyDamage(modifiedDamage); //Apply the damage. Critical hits deal double.
                if (damageDealt > 0)
                    ObjectSpawner.SpawnDmgPopup(other.transform.position, (int) damageDealt,
                        crit); //Spawn a popup for the damage text if the damage is greater than zero.
            }


            var kb = GetKnockbackVector(other.transform.position) * KnockbackForce * knockbackMultiplier; //Calculate the knockback

            if (kb.x != 0 || kb.y != 0)
            {
                if(entity != null) entity.ApplyKnockbackGlobal(kb); //Apply the knockback
            }

            _hitCooldowns.Add(other.gameObject, damageCooldown); //Put the object on cooldown

            //Add inflict buffs and effects
            if (entity != null)
            {
                if (inflictBuffs != null) //Inflict buffs...
                    foreach (var b in inflictBuffs)
                    {
                        entity.Stats.AddBuff(new StatBonus(b.statType, b.stat, b.strength, GetInstanceID().ToString()),
                            damageCooldown);
                    }

                if (inflictEffects != null && netId == null) //Inflict effects...
                    foreach (var e in inflictEffects)
                    {
                        entity.AddEffect(GameEffect.FromString(e), _owner);
                    }
            }
            //

            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (destroyOnAnyCollide && other.gameObject != _owner.gameObject)
                Destroy(gameObject); //Destroy if destroyOnAnyCollide is true.
            if (!_hitCooldowns.ContainsKey(other.gameObject))
            {
                TryDamage(other); //On trigger enter, try to damage the other collider.
            }
        }

        #endregion

        #region Accessors

        //Adds an effect to the list of inflicting effects
        public void AddInflictEffect(string effect)
        {
            inflictEffects.Add(effect);
        }

        //Adds an effect from the list of inflicting effects
        public void RemoveInflictEffect(string effect)
        {
            inflictEffects.Remove(effect);
        }

        private bool _set;
        
        //Sets necessary data when given AttackData and GameEntity. To be called upon being instantiated as part of an Entity's attack.
        public void SetData(AttackData attackData, GameEntity owner)
        {
            if (_set)
            {
                Debug.LogWarning("AttackComponent has been set twice!");
                return;
            }
            
            damageMultiplier = attackData?.damageMultiplier ?? damageMultiplier;
            knockbackMultiplier = attackData?.knockbackMultiplier ?? knockbackMultiplier;
            team = attackData?.team ?? team;
            if(damageCooldown == 0) damageCooldown = 0.5f;
            destroyOnApply = attackData?.destroyOnApply ?? destroyOnApply;
            _owner = owner;

            _set = true;
        }

        #endregion

        //Gets a knockback vector. Either based on a vector between the attack and other or based on the RigidBody's velocity.
        private Vector2 GetKnockbackVector(Vector2 otherPos)
        {
            var velocity = _velocity.normalized;
            if (velocity.x == 0 && velocity.y == 0)
            {
                return otherPos - (Vector2)transform.position;
            }
            return _velocity.normalized;
        }
    }
}