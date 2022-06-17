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
    [SerializeField] private int damage;
    [SerializeField] private Team team;
    [SerializeField] private bool destroyOnApply;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float damageCooldown;
    private float damageTimer;
    private Vector2 knockbackVector = Vector2.zero;

    private void Update()
    {
        damageTimer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //
        var damageable = other.gameObject.GetComponent<IDamageable>();
        var rb = other.gameObject.GetComponent<Rigidbody2D>();
        //
        if (damageable == null || damageable.team == team || damageTimer <= damageCooldown) return; //Check if valid thing to hit

        damageable.ApplyDamage(damage); //Apply the damage
        
        damageTimer = 0; //Reset the timer

        Vector2 kb = GetKnockbackVector(other.gameObject) * knockbackForce; //Calculate the knockback
        
        if(rb != null) rb.AddForce(kb); //Apply the knockback
        
        if (destroyOnApply) Destroy(this); //Destroy when done if that option is selected
    }

    private Vector2 GetKnockbackVector(GameObject go)
    {
        if (knockbackVector != Vector2.zero) return knockbackVector;
        return (go.transform.position - transform.position).normalized;
    }

    public void SetKnockbackVector(Vector2 vector)
    {
        knockbackVector = vector;
    }
}
