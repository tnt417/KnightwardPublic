using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [NonSerialized] public int damage;
    [NonSerialized] public bool damagePlayer; //TODO: enemies can damage other enemies rn, if they ever shoot

    private void OnCollisionEnter2D(Collision2D other)
    {
        var damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable == null || !damagePlayer && other.gameObject.layer == LayerMask.NameToLayer("Player")) return;
        damageable.ApplyDamage(damage);
        Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
