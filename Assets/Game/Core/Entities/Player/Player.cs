using System;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.U2D;

namespace TonyDev.Game.Core.Entities.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : GameEntity
    {
        public static event Action OnLocalPlayerCreated;
        
        //Singleton code
        public static Player LocalInstance;

        //Sub-components of the player, for easy access.
        public PlayerMovement playerMovement;
        public PlayerAnimator playerAnimator;
        public PlayerDeath playerDeath;

        protected override bool CanAttack => Input.GetMouseButton(0) && base.CanAttack && !playerDeath.dead;

        //All implementations of IDamageable contained in a region

        #region IDamageable

        public override Team Team => Team.Player;

        #endregion

        [SyncVar(hook=nameof(UsernameHook))] public string username;

        public Action<string> OnUsernameChange;
        
        public void UsernameHook(string oldUser, string newUser)
        {
            OnUsernameChange?.Invoke(username);
        }
        
        public override void OnStartServer()
        {
            CmdSetUsername(LobbyManager.UsernameDict[netIdentity.connectionToClient.connectionId]);
            base.OnStartServer();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetUsername(string user)
        {
            username = user;
        }
        
        public override void OnStartLocalPlayer()
        {
            Team = Team.Player;
            LocalInstance = this;
            OnLocalPlayerCreated?.Invoke();

            PlayerStats.Stats = Stats;

            OnAttack += () =>
            {
                var projectileData = PlayerInventory.Instance.WeaponItem.projectiles;

                if (projectileData == null) return;

                foreach (var proj in projectileData)
                {
                    var direction = GameManager.MouseDirection;
                    var pos = transform.position;

                    var identifier = AttackComponent.GetUniqueIdentifier(this);
                    AttackFactory.CreateProjectileAttack(this, pos, direction, proj, identifier);
                    GameManager.Instance.CmdSpawnProjectile(netIdentity, pos, GameManager.MouseDirection, proj, identifier);
                }
            };
            
            Init();
            
            PlayerInventory.Instance.InsertStarterItems();
        }

        [GameCommand(Keyword = "god", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Toggled invulnerability.")]
        public static void ToggleInvulnerable()
        {
            LocalInstance.IsInvulnerable = !LocalInstance.IsInvulnerable;
        }
    }
}