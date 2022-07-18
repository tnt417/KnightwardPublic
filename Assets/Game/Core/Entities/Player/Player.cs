using System;
using System.Transactions;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Player.Combat;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : GameEntity
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
        public override Team Team => Team.Player;
        public override int MaxHealth => (int)PlayerStats.Health;

        public override void ApplyDamage(float damage)
        {
            if (PlayerStats.DodgeSuccessful) return; //Don't apply damage is dodge rolls successful.
            var modifiedDamage = damage >= 0
                ? Mathf.Clamp(PlayerStats.ModifyIncomingDamage(damage), 0, Mathf.Infinity)
                : damage;
            base.ApplyDamage(damage);
            OnPlayerDamage?.Invoke(modifiedDamage);
        }

        public override void Die()
        {
            playerDeath.Die();
        }

        #endregion

        public void Update()
        {
            //Enable/disable parts of the player depending on if player is alive //TODO not good
            playerMovement.enabled = IsAlive;
            PlayerCombat.Instance.gameObject.SetActive(IsAlive);
            //
            
            if (IsAlive)
            {
                var hpRegen = MaxHealth * 0.01f * Time.deltaTime; //Regen 1% of hp per second
                ApplyDamage(-hpRegen); //Regen health by HpRegen per second
            }
        }

        public void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        
            DontDestroyOnLoad(gameObject); //Player persists between scenes
            
            GameManager.Entities.Add(this);
            
            CurrentHealth = MaxHealth;
        }
    }
}