using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public void ApplyDamage(int damage)
    {
        if (Random.Range(0f, 1f) < PlayerStats.GetStatBonus(Stat.Dodge)) return; //Don't apply damage is dodge rolls successful.
        CurrentHealth -=
            (int) Mathf.Clamp(
                (damage - PlayerStats.GetStatBonus(Stat.Armor) * 10f) * (1f - PlayerStats.GetStatBonus(Stat.DamageReduction)),
                0, Mathf.Infinity); //Modify the incoming damage. Armor is a flat damage reduction, damage reduction is a percentage.
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
        MaxHealth = 100 + (int)(PlayerStats.GetStatBonus(Stat.Health) * 100); //Health bonus is a percentage boost of the base health of 100
        if (IsAlive)
        {
            CurrentHealth +=
                (1f + PlayerStats.GetStatBonus(Stat.HpRegen)) * Time.deltaTime; //Regen health by 1 + HpRegen per second
            OnHealthChanged?.Invoke(); //Since health was regenerated, invoke this.
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); //Clamp health
    }

    public void Awake()
    {
        //Singleton code
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
        //
        
        DontDestroyOnLoad(gameObject); //Player persists between scenes
        
        //Set the player's health. Hardcoded here.
        MaxHealth = 100 + (int)(PlayerStats.GetStatBonus(Stat.Health) * 100);
        CurrentHealth = MaxHealth;
    }

    public void SetHealth(int newHealth) //TODO: If really picky, should convert this to be within the setters of the variable instead.
    {
        CurrentHealth = newHealth;
        OnHealthChanged?.Invoke();
    }

}