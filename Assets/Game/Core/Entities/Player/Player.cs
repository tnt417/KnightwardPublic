using System;
using TonyDev.Game.Core.Combat;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : MonoBehaviour, IDamageable
    {
        //Singleton code
        public static Player Instance;
    
        //Sub-components of the player, for easy access.
        public PlayerMovement playerMovement;
        public PlayerAnimator playerAnimator;
        public PlayerDeath playerDeath;

        public delegate void DamageCallback(float damage);
        public static event DamageCallback OnPlayerDamage;
        
        //All implementations of IDamageable contained in a region
        #region IDamageable

        public Team team => Team.Player;
        public int MaxHealth => (int)PlayerStats.Health;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public void ApplyDamage(int damage)
        {
            if (PlayerStats.DodgeSuccessful) return; //Don't apply damage is dodge rolls successful.
            var modifiedDamage = Mathf.Clamp(PlayerStats.ModifyIncomingDamage(damage), 0, Mathf.Infinity);
            CurrentHealth -= modifiedDamage;
            
            //Broadcast that health was changed and that player was damaged
            OnHealthChanged?.Invoke();
            OnPlayerDamage?.Invoke(modifiedDamage);
        
            playerAnimator.PlayHurtAnimation();
        }

        public void Die()
        {
            playerDeath.Die();
        }

        public event IDamageable.HealthAction OnHealthChanged;

        #endregion

        public void Update()
        {
            if (CurrentHealth <= 0)
            {
                Die();
            }
            
            //Enable/disable parts of the player depending on if player is alive
            playerMovement.enabled = IsAlive;
            PlayerCombat.Instance.gameObject.SetActive(IsAlive);
            //
            
            if (IsAlive)
            {
                CurrentHealth += PlayerStats.HpRegen * Time.deltaTime; //Regen health by HpRegen per second
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
            
            CurrentHealth = MaxHealth;
        }

        public void SetHealth(int newHealth) //TODO: If really picky, should convert this to be within the setters of the variable instead.
        {
            CurrentHealth = newHealth;
            OnHealthChanged?.Invoke();
        }

    }
}