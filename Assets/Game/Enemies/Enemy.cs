using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(IEnemyMovement))]
public class Enemy : MonoBehaviour, IDamageable
{
    //Editor variables
    [SerializeField] private string hurtAnimationName;
    [SerializeField] private Animator animator;
    [SerializeField] private int maxHealth;
    //

    //Contains all interface code for IDamageable
    #region IDamageable
    public Team team => Team.Enemy;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    [NonSerialized] public Transform Target;
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
        GameManager.Enemies.Remove(this);
        Destroy(gameObject);
    }
    public event IDamageable.HealthAction OnHealthChanged;
    #endregion
    
    public void Awake()
    {
        GameManager.Enemies.Add(this); //Add this enemy to the GameManager's enemy list.
        
        //Initialize health variables.
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
    }

    public GameObject UpdateTarget() //Updates enemy's target and returns it.
    {
        var go = FindObjectsOfType
                <MonoBehaviour>()
            .Where(mb => (mb as IDamageable)?.team == Team.Player && ((IDamageable) mb).IsAlive)
            .OrderBy( mb => Vector2.Distance(mb.transform.position, transform.position))
            .FirstOrDefault()
            ?.gameObject; //Finds closest non-dead damageable object on the player team
        
        Target = go == null ? null : go.transform; //Update the Target variable
        
        return go; //Returns the game object
    }
}
