using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private const float KnockbackForce = 10f; //A multiplier so knockback values can be for example 20 instead of 20000

        #region Inspector Variables

        [Tooltip("The base damage of the attack. May be altered based on the owner entity's stats.")] [SerializeField]
        public float damage;
        
        [Tooltip("The type of the attack.")] [SerializeField]
        public DamageType damageType;

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
        
        [Tooltip("Should the object destroy when its collider collides?")] [SerializeField]
        public bool destroyOnHitWall = false;

        [Tooltip("Does the attack damage invincible entities?")] [SerializeField]
        public bool ignoreInvincibility = false;

        [Tooltip("Buffs inflicted to GameEntities upon applying damage")] [SerializeField]
        private List<StatBonus> inflictBuffs = new();

        [Tooltip("Effects inflicted to GameEntities upon applying damage")] [SerializeField]
        private List<GameEffect> inflictEffects = new();

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

        private Vector3 _lastScale;
        
        private void FixedUpdate () {
            _velocity = (Vector2)transform.position - _oneFrameAgo;
            _oneFrameAgo = transform.position;

            var newScale = Vector3.one * (_owner == null ? 1 : _owner.Stats.GetStat(Stat.AoeSize));

            if (_lastScale != newScale) transform.localScale = newScale;
            
            _lastScale = transform.localScale;
        }
        
        private void Start()
        {
            if(rb2d == null) rb2d = GetComponent<Rigidbody2D>();
            if (rb2d == null)
            {
                rb2d = gameObject.AddComponent<Rigidbody2D>();
                rb2d.isKinematic = true; //Add a RigidBody if there isn't one
            }

            if (_owner == null)
                _owner = transform.root.GetComponent<GameEntity>(); //Owner is our own GameEntity if it hasn't been set yet.
        }

        private void Update()
        {
            var hitObjects = _hitCooldowns.ToArray();
            
            foreach (var (go, cd) in hitObjects)
            {
                if (go == null)
                {
                    _hitCooldowns.Remove(go);
                    continue;
                }

                if (cd < Time.time)
                {
                    _hitCooldowns.Remove(go);
                    CheckCollision(go);
                }
            }
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
            if(NetworkClient.active) GameManager.Instance.CmdDestroyProjectile(identifier);
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

        public Action<float, GameEntity, bool, DamageType> OnDamageDealt;
        
        private void TryDamage(Collider2D other)
        {
            if (!NetworkClient.active) return;
            
            var damageable = other.GetComponent<IDamageable>();

            if (damageable == null) return;
            
            var ge = damageable is GameEntity entity ? entity : null;
            
            //Don't want to return in cases where the thing being hit is local player, so hit detection doesn't have latency with the player.
            if (_owner != null && !_owner.isOwned && (ge == null || ge is not Player))
                return; //Only call damage code on attacks that are owned by our client.

            var go = other.gameObject;
            
            var onCooldown = _hitCooldowns.ContainsKey(go) && _hitCooldowns[go] > Time.time;

            //
            if (damageable.Team == team || !damageable.IsTangible ||
                onCooldown || !other.isTrigger) return; //Check if valid thing to hit

            _hitCooldowns[go] = Time.time + damageCooldown; //Put the object on cooldown

            var crit = IsCriticalHit;
            
            float modifiedDamage = (int) (Damage * damageMultiplier *
                                          (crit
                                              ? 2
                                              : 1));

            if (ge is Player && ge != Player.LocalInstance) return;

            float damageDealt = 0;
            
            if (ge != null)
            {
                var success = true;
                damageDealt = ge.isLocalPlayer
                    ? ge.ApplyDamage(modifiedDamage, out success, ignoreInvincibility)
                    : modifiedDamage; //Do damage before command if hitting player
                if (!success) return;
                if (ge is Enemy && !NetworkServer.active && !ge.IsInvulnerable) ge.ClientHealthDisparity -= damageDealt;
                ge.LocalHurt(damageDealt, crit, damageType);
                ge.CmdDamageEntity(damageDealt, crit, NetworkClient.localPlayer, ignoreInvincibility, damageType);
                if(!ge.IsInvulnerable) OnDamageDealt?.Invoke(damageDealt, ge, crit, damageType);
            }
            else
            {
                damageDealt =
                    damageable.ApplyDamage(modifiedDamage, out var success, ignoreInvincibility); //Apply the damage. Critical hits deal double.
                if (damageDealt > 0 && success)
                    ObjectSpawner.SpawnDmgPopup(other.transform.position, damageDealt,
                        crit, damageType); //Spawn a popup for the damage text if the damage is greater than zero.
                if (!success) return;
            }

            // var kb = GetKnockbackVector(other.transform.position) * (KnockbackForce * knockbackMultiplier); //Calculate the knockback

            var attSpd = _owner != null ? _owner.Stats.GetStat(Stat.AttackSpeed) : 0;

            // if (kb.x != 0 || kb.y != 0)
            // {
                // if (attSpd > 0) kb /= attSpd;
                // //Debug.Log("Applying kb for: " + gameObject.name);
                //if(ge != null) ge.ApplyKnockbackGlobal(kb); //Apply the knockback
            // }

            var percentDealt = damage / ge.NetworkMaxHealth;
            
            var eb = ge.GetComponent<EnemyBehavior>();
            if (eb != null)
            {
                if(percentDealt > 0.05) eb.PauseMovement(0.5f);
                eb.Dash((Random.Range(0.5f, 0.7f) + percentDealt * 0.5f) * knockbackMultiplier,
                    GameTools.Rotate(ge.transform.position - transform.position, Random.Range(-0.2f, 0.2f))
                        .normalized);
            }

            //Add inflict buffs and effects
            if (ge != null && !ge.IsInvulnerable)
            {
                if (inflictBuffs != null) //Inflict buffs...
                    foreach (var b in inflictBuffs)
                    {
                        ge.Stats.AddBuff(new StatBonus(b.statType, b.stat, b.strength, GetInstanceID().ToString()),
                            damageCooldown);
                    }

                if (inflictEffects != null)
                {
                    //Inflict effects...
                    foreach (var e in inflictEffects)
                    {
                        if (ge.isOwned)
                        {
                            ge.AddEffect(e, _owner);
                        }
                        else
                        {
                            ge.CmdAddEffect(e, _owner);
                        }
                    }
                }
            }
            //

            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (destroyOnAnyCollide && other.gameObject != _owner.gameObject)
                Destroy(gameObject); //Destroy if destroyOnAnyCollide is true.
            
            TryDamage(other); //On trigger enter, try to damage the other collider.
        }

        #endregion

        #region Accessors

        //Adds an effect to the list of inflicting effects
        public void AddInflictEffect(GameEffect effect)
        {
            inflictEffects.Add(effect);
        }

        //Adds an effect from the list of inflicting effects
        public void RemoveInflictEffect(GameEffect effect)
        {
            inflictEffects.Remove(effect);
        }

        private bool _set;
        
        //Sets necessary data when given AttackData and GameEntity. To be called upon being instantiated as part of an Entity's attack.
        public void SetData(AttackData attackData, GameEntity owner)
        {
            if (_set)
            {
                //Debug.LogWarning("AttackComponent has been set twice!");
                return;
            }
            
            damageMultiplier = attackData?.damageMultiplier ?? damageMultiplier;
            knockbackMultiplier = attackData?.knockbackMultiplier ?? knockbackMultiplier;
            team = attackData?.team ?? team;
            ignoreInvincibility = attackData?.ignoreInvincibility ?? ignoreInvincibility;

            if (team == owner.Team) //If the attack's team is the same as the owner's team...
            {
                owner.OnTeamChange += (newTeam) => team = newTeam; //Change the attack's team when the owner's team is changed.
            }
            
            if(damageCooldown == 0) damageCooldown = 0.5f;
            destroyOnApply = attackData?.destroyOnApply ?? destroyOnApply;
            _owner = owner;
            
            //Debug.Log("Owner setting to: " + _owner.name + " for " + gameObject.name);

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