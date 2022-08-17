using System;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;
using UnityEngine.U2D;

namespace TonyDev.Game.Core.Entities.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : GameEntity
    {
        //Singleton code
        public static Player LocalInstance;

        //Sub-components of the player, for easy access.
        public PlayerMovement playerMovement;
        public PlayerAnimator playerAnimator;
        public PlayerDeath playerDeath;

        protected override bool CanAttack => Input.GetMouseButton(0) && base.CanAttack;

        //All implementations of IDamageable contained in a region

        #region IDamageable

        public override Team Team => Team.Player;

        #endregion

        private new void Update()
        {
            base.Update();
            //Enable/disable parts of the player depending on if player is alive //TODO not good
            playerMovement.enabled = IsAlive;
            //
        }

        public override void Die()
        {
            playerDeath.Die();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private new void Start()
        {
            base.Start();
            
            Debug.Log($"Player started. Has authority? {hasAuthority}");
            //Singleton code
            if (LocalInstance == null && LocalInstance != this && hasAuthority) LocalInstance = this;
            else return;
            //

            PlayerStats.Stats = Stats;

            DontDestroyOnLoad(gameObject); //Player persists between scenes

            //Add base stat bonuses
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.MoveSpeed, 5.0f, "Player");
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.AoeSize, 1.0f, "Player");
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.Health, 100f, "Player");
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.HpRegen, 1f, "Player");

            OnAttack += () =>
            {
                var projectileData = PlayerInventory.Instance.WeaponItem.projectiles;

                if (projectileData == null) return;

                foreach (var proj in projectileData)
                    GameManager.Instance.CmdSpawnProjectile(netIdentity, GameManager.MouseDirection, proj);
            };

            CurrentHealth = MaxHealth;
            CmdSetHealth(CurrentHealth, MaxHealth);
        }

        [GameCommand(Keyword = "god", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Toggled invulnerability.")]
        public static void ToggleInvulnerable()
        {
            LocalInstance.IsInvulnerable = !LocalInstance.IsInvulnerable;
        }
    }
}