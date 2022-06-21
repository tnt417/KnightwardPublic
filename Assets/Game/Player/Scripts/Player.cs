using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour, IDamageable
{
    //Singleton code
    public static Player Instance;
    
    //Sub-components of the player, for easy access.
    public PlayerMovement playerMovement;
    public PlayerAnimator playerAnimator;
    public PlayerDeath playerDeath;

    //All implementations of IDamageable contained in a region
    #region IDamageable

    public Team team => Team.Player;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke();
        
        playerAnimator.PlayHurtAnimation();
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        playerDeath.Die();
    }

    public event IDamageable.HealthAction OnHealthChanged;

    #endregion

    public void Update()
    {
        //Enable/disable parts of the player depending on if player is alive
        playerMovement.enabled = IsAlive;
        PlayerCombat.Instance.gameObject.SetActive(IsAlive);
    }
    
    public void Awake()
    {
        //Singleton code
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
        //
        
        DontDestroyOnLoad(gameObject); //Player persists between scenes
        
        //Set the player's health. Hardcoded here.
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public void SetHealth(int newHealth) //TODO: If really picky, should convert this to be within the setters of the variable instead.
    {
        CurrentHealth = newHealth;
        OnHealthChanged?.Invoke();
    }

}