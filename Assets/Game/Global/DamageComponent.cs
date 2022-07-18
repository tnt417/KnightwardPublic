using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Movement;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Combat
{
    public enum Team
    {
        Enemy,
        Player,
        Environment
    }

    [RequireComponent(typeof(Collider2D))]
    public class DamageComponent : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public float damage;
        [SerializeField] public float damageMultiplier = 1;
        [SerializeField] public float damageCooldown;
        [SerializeField] public Team team;
        [SerializeField] public bool destroyOnApply;
        [SerializeField] public bool destroyOnAnyCollide;
        [SerializeField] private float knockbackForce = 1000f;
        [SerializeField] public float knockbackMultiplier = 1;
        [SerializeField] public float enemySpeedModifier = 1f;
        //

        private Rigidbody2D _rb2d;
        [NonSerialized] public Vector2 KnockbackVector = Vector2.zero;
        private Dictionary<GameObject, float> _hitCooldowns = new();
        [NonSerialized] public GameObject Owner;
        
        private void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            if (Owner == null) Owner = gameObject;
        }
        
        private void Update()
        {
            if (team == Team.Enemy && damage > 0)
            {
                damageMultiplier = 1 + Mathf.Log10(GameManager.EnemyDifficultyScale) - 0.5f;
            }

            //Tick hit cooldown timers
            Dictionary<GameObject, float> newHitCooldowns = new();

            foreach (var go in _hitCooldowns.Keys)
            {
                var newTime = _hitCooldowns[go] - Time.deltaTime;
                if (newTime > 0) newHitCooldowns.Add(go, newTime);
            }

            _hitCooldowns = newHitCooldowns;
            //
        }
        

        private void OnTriggerStay2D(Collider2D other)
        {
            if(destroyOnAnyCollide && other.gameObject != Owner) Destroy(gameObject);
            //
            var damageable = other.gameObject.GetComponent<GameEntity>();
            var rb = other.gameObject.GetComponent<Rigidbody2D>();
            //
            if (damageable == null || damageable.Team == team || damageable.IsInvulnerable || _hitCooldowns.ContainsKey(other.gameObject) ||
                !other.isTrigger) return; //Check if valid thing to hit

            damageable.ApplyDamage((int) (damage * damageMultiplier)); //Apply the damage

            var kb = GetKnockbackVector(other.transform) * knockbackForce *
                     knockbackMultiplier; //Calculate the knockback

            if (rb != null) rb.AddForce(kb); //Apply the knockback

            _hitCooldowns.Add(other.gameObject, damageCooldown); //Put the object on cooldown

            //TODO temporary implementation
            var move = other.GetComponent<EnemyMovementBase>();
            if (move != null && enemySpeedModifier != 0) move.StartCoroutine(move.ModifySpeedForSeconds(enemySpeedModifier, damageCooldown));
            //
            
            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private Vector2 GetKnockbackVector(Transform other)
        {
            if (KnockbackVector != Vector2.zero)
                return KnockbackVector; //If knockback vector has been set, return the pre-calculated vector.
            return _rb2d == null ? (other.position - transform.position).normalized : _rb2d.velocity.normalized; //Otherwise, return a calculated vector.
        }
    }
}