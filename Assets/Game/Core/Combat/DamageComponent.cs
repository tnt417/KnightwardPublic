using System;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Combat
{
    public enum Team
    {
        Enemy,Player
    }

    [RequireComponent(typeof(Collider2D))]
    public class DamageComponent : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private int damage;
        [SerializeField] public float damageMultiplier = 1;
        [SerializeField] public float damageCooldown;
        [SerializeField] private Team team;
        [SerializeField] private bool destroyOnApply;
        [SerializeField] private float knockbackForce;
        [SerializeField] public float knockbackMultiplier = 1;
        //
    
        private float _damageTimer;
        [NonSerialized] public Vector2 KnockbackVector = Vector2.zero;

        private void Awake()
        {
            _damageTimer = damageCooldown;
        }
        
        private void Update()
        {
            _damageTimer += Time.deltaTime;
            if (team == Team.Enemy)
            {
                damageMultiplier = 1 + Mathf.Log10(GameManager.EnemyDifficultyScale) - 0.5f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //
            var damageable = other.gameObject.GetComponent<IDamageable>();
            var rb = other.gameObject.GetComponent<Rigidbody2D>();
            //
            if (damageable == null || damageable.team == team || _damageTimer <= damageCooldown || !other.isTrigger) return; //Check if valid thing to hit

            damageable.ApplyDamage((int)(damage * damageMultiplier)); //Apply the damage
        
            _damageTimer = 0; //Reset the timer

            var kb = GetKnockbackVector(other.gameObject) * knockbackForce * knockbackMultiplier; //Calculate the knockback

            if(rb != null) rb.AddForce(kb); //Apply the knockback
        
            if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
        }

        private Vector2 GetKnockbackVector(GameObject go)
        {
            if (KnockbackVector != Vector2.zero) return KnockbackVector; //If knockback vector has been set, return the pre-calculated vector.
            return (go.transform.position - transform.position).normalized; //Otherwise, return a calculated vector.
        }
    }
}