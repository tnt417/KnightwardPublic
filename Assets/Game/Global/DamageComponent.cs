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

        [SerializeField] public List<StatBonus> inflictBuffs;
        //

        public Rigidbody2D rb2d;
        [NonSerialized] public bool IsCriticalHit = false;
        [NonSerialized] public Vector2 KnockbackVector = Vector2.zero;
        private Dictionary<GameObject, float> _hitCooldowns = new();
        [NonSerialized] public GameObject Owner;

        private void Start()
        {
            if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
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
                else
                {
                    _hitCooldowns = newHitCooldowns;
                    CheckCollision(go);
                }
            }

            _hitCooldowns = newHitCooldowns;
            //
        }

        private void CheckCollision(GameObject go)
        {
            var otherTrigger = go.GetComponents<Collider2D>().FirstOrDefault(c2d => c2d.isTrigger);
            if (rb2d != null && rb2d.IsTouching(otherTrigger))
                TryDamage(otherTrigger);
        }

        private void TryDamage(Collider2D other)
        {
            var damageable = other.gameObject.GetComponent<IDamageable>();
            //
            if (damageable == null || damageable.Team == team || damageable.IsInvulnerable ||
                _hitCooldowns.ContainsKey(other.gameObject) ||
                !other.isTrigger) return; //Check if valid thing to hit

            damageable.ApplyDamage((int) (damage * damageMultiplier)); //Apply the damage
            if (damage * damageMultiplier > 0)
                PopupManager.SpawnPopup(other.transform.position, (int) (damage * damageMultiplier),
                    IsCriticalHit);


            var kb = GetKnockbackVector(other.transform) * knockbackForce *
                     knockbackMultiplier; //Calculate the knockback

            if (kb.sqrMagnitude > 0)
            {
                var rb = other.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null) rb.AddForce(kb); //Apply the knockback
            }

            _hitCooldowns.Add(other.gameObject, damageCooldown); //Put the object on cooldown

            //Renew inflict buffs
            var entity = other.GetComponent<GameEntity>();
            if (entity && inflictBuffs != null)
            {
                foreach (var b in inflictBuffs)
                {
                    entity.Buff.RemoveStatBonuses(GetInstanceID().ToString());
                    entity.Buff.AddStatBonus(b.statType, b.stat, b.strength, GetInstanceID().ToString());
                }
            }
            //

            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (destroyOnAnyCollide && other.gameObject != Owner) Destroy(gameObject);
            if (!_hitCooldowns.ContainsKey(other.gameObject))
            {
                TryDamage(other);
            }
        }

        private Vector2 GetKnockbackVector(Transform other)
        {
            if (KnockbackVector != Vector2.zero)
                return KnockbackVector; //If knockback vector has been set, return the pre-calculated vector.
            return rb2d == null
                ? (other.position - transform.position).normalized
                : rb2d.velocity.normalized; //Otherwise, return a calculated vector.
        }
    }
}