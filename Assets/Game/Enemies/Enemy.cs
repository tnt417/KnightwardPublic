using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(IEnemyMovement))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private string hurtAnimationName;
    [SerializeField] private Animator animator;
    [SerializeField] private int maxHealth;
    public Team team => Team.Enemy;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public Transform target;
    public void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke();
        animator.Play(hurtAnimationName);
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public event IDamageable.HealthAction OnHealthChanged;

    public void Awake()
    {
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
    }

    public GameObject UpdateTarget()
    {
        var go = FindObjectsOfType
                <MonoBehaviour>()
            .Where(mb => (mb as IDamageable)?.team == Team.Player && ((IDamageable) mb).IsAlive)
            .OrderBy( mb => Vector2.Distance(mb.transform.position, transform.position))
            .FirstOrDefault()
            ?.gameObject;
        if (go == null) return null;
        target = go.transform;
        return go;
    }
}
