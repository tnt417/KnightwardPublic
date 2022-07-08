using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Combat
{
    public enum Team
    {
        Enemy,
        Player
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
        [SerializeField] private float knockbackForce = 1000f;

        [SerializeField] public float knockbackMultiplier = 1;
        //

        [NonSerialized] public Vector2 KnockbackVector = Vector2.zero;
        private Dictionary<GameObject, float> _hitCooldowns = new();

        private void Update()
        {
            if (team == Team.Enemy && damage > 0)
            {
                damageMultiplier = 1 + Mathf.Log10(GameManager.EnemyDifficultyScale) - 0.5f;
            }

            if (team == Team.Enemy) knockbackMultiplier = 1 * PlayerStats.NegativeEffectMultiplier;

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
            //
            var damageable = other.gameObject.GetComponent<GameEntity>();
            var rb = other.gameObject.GetComponent<Rigidbody2D>();
            //
            if (damageable == null || damageable.Team == team || damageable.IsInvulnerable || _hitCooldowns.ContainsKey(other.gameObject) ||
                !other.isTrigger) return; //Check if valid thing to hit

            damageable.ApplyDamage((int) (damage * damageMultiplier)); //Apply the damage

            var kb = GetKnockbackVector(other.gameObject) * knockbackForce *
                     knockbackMultiplier; //Calculate the knockback

            if (rb != null) rb.AddForce(kb); //Apply the knockback

            _hitCooldowns.Add(other.gameObject, damageCooldown); //Put the object on cooldown

            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private Vector2 GetKnockbackVector(GameObject go)
        {
            if (KnockbackVector != Vector2.zero)
                return KnockbackVector; //If knockback vector has been set, return the pre-calculated vector.
            return (go.transform.position - transform.position).normalized; //Otherwise, return a calculated vector.
        }
    }
}