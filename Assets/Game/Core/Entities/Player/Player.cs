using System;
using System.Collections;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.UI.Minimap;
using UnityEngine;

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

        [SyncVar(hook = nameof(UsernameHook))] public string username;

        public Action<string> OnUsernameChange;

        private bool _damageOnCooldown;
        private const float PlayerDamageCooldown = 0.1f;

        protected override void ParentIdentityHook(NetworkIdentity oldIdentity, NetworkIdentity newIdentity)
        {
            base.ParentIdentityHook(oldIdentity, newIdentity);

            if (!isServer) return;
            
            if (oldIdentity != null)
            {
                var oldRoom = RoomManager.Instance.GetRoomFromID(oldIdentity.netId);
                if (oldRoom != null) oldRoom.CmdSetPlayerCount(oldRoom.PlayerCount - 1);
            }

            if (newIdentity != null)
            {
                var newRoom = RoomManager.Instance.GetRoomFromID(newIdentity.netId);
                if (newRoom != null) newRoom.CmdSetPlayerCount(newRoom.PlayerCount + 1);
            }
        }

        public override float ApplyDamage(float damage, out bool successful, bool ignoreInvincibility)
        {
            if (damage > 0 && _damageOnCooldown && !ignoreInvincibility)
            {
                successful = false;
                return 0;
            }

            if (!ignoreInvincibility) StartCoroutine(PauseDamageForSeconds(PlayerDamageCooldown));

            return base.ApplyDamage(damage, out successful, ignoreInvincibility);
        }

        private IEnumerator PauseDamageForSeconds(float seconds)
        {
            _damageOnCooldown = true;
            yield return new WaitForSeconds(seconds);
            _damageOnCooldown = false;
        }

        public void UsernameHook(string oldUser, string newUser)
        {
            OnUsernameChange?.Invoke(username);
        }

        public override void OnStartServer()
        {
            CmdSetUsername(CustomRoomPlayer.Local.username);
            base.OnStartServer();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetUsername(string user)
        {
            username = user;
        }

        public static Action<NetworkIdentity> LocalPlayerChangeIdentity;
        
        public override void OnStartLocalPlayer()
        {
            Team = Team.Player;
            LocalInstance = this;
            OnLocalPlayerCreated?.Invoke();
            
            OnParentIdentityChange += (a) => LocalPlayerChangeIdentity?.Invoke(a);

            PlayerStats.Stats = Stats;

            OnAttack += () =>
            {
                var projectileData = PlayerInventory.Instance.WeaponItem.projectiles;

                if (projectileData == null) return;

                foreach (var proj in projectileData)
                {
                    var pos = transform.position;

                    ObjectSpawner.SpawnProjectile(this, pos, GameManager.MouseDirection, proj);
                }
            };

            //OnDamageOther += (_, _, _) => SoundManager.PlaySoundPitchVariant("hit", transform.position, 0.7f, 1f);

            Init();

            PlayerInventory.Instance.InsertStarterItems();
            
            TransitionController.Instance.FadeIn();
        }

        [GameCommand(Keyword = "god", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Toggled invulnerability.")]
        public static void ToggleInvulnerable()
        {
            LocalInstance.IsInvulnerable = !LocalInstance.IsInvulnerable;
        }
    }
}