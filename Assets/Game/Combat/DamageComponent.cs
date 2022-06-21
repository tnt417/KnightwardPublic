using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public enum Team
{
    Enemy,Player
}

[RequireComponent(typeof(Collider2D))]
public class DamageComponent : MonoBehaviour
{
    //Editor variables
    [SerializeField] private int damage;
    [SerializeField] public float damageMultiplier = 0;
    [SerializeField] private float damageCooldown;
    [SerializeField] private Team team;
    [SerializeField] private bool destroyOnApply;
    [SerializeField] private float knockbackForce;
    [SerializeField] public float knockbackMultiplier = 0;

    //
    private float _damageTimer;
    [NonSerialized] public Vector2 knockbackVector = Vector2.zero;

    private void Update()
    {
        _damageTimer += Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //
        var damageable = other.gameObject.GetComponent<IDamageable>();
        var rb = other.gameObject.GetComponent<Rigidbody2D>();
        //
        if (damageable == null || damageable.team == team || _damageTimer <= damageCooldown) return; //Check if valid thing to hit

        damageable.ApplyDamage((int)(damage * damageMultiplier)); //Apply the damage
        
        _damageTimer = 0; //Reset the timer

        Vector2 kb = GetKnockbackVector(other.gameObject) * knockbackForce * (1+knockbackMultiplier); //Calculate the knockback
        
        if(rb != null) rb.AddForce(kb); //Apply the knockback
        
        if (destroyOnApply) Destroy(gameObject); //Destroy when done if that option is selected
    }

    private Vector2 GetKnockbackVector(GameObject go)
    {
        if (knockbackVector != Vector2.zero) return knockbackVector; //If knockback vector has been set, return the pre-calculated vector.
        return (go.transform.position - transform.position).normalized; //Otherwise, return a calculated vector.
    }
}
