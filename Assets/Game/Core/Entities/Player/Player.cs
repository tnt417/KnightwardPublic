using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.UI.Tower;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

        public bool stopPlayerAttack = false;
        public bool fireKeyHeld = false;
        public override bool CanAttack => (!stopPlayerAttack) && fireKeyHeld && base.CanAttack && !playerDeath.dead && !PauseController.Paused && !TowerPlacementManager.Instance.BlockAttacksFromTowerPlacement;

        public void SetAttackProgress(float normalized)
        {
            AttackTimer = normalized * AttackTimerMax;
        }

        public void OnFire(InputValue value)
        {
            if (!isOwned) return;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                fireKeyHeld = false;
                return;
            }
            
            AttackTimer = 0f;
            fireKeyHeld = value.isPressed;
        }

        //All implementations of IDamageable contained in a region

        #region IDamageable

        public override Team Team => Team.Player;

        #endregion

        [SyncVar(hook = nameof(UsernameHook))] public string username;

        public Action<string> OnUsernameChange;

        private bool _damageOnCooldown;
        private const float PlayerDamageCooldown = 0.25f;

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

        public void DisableCollisionUntil(Func<bool> condition)
        {
            DisableCollisionTask(condition).Forget();
        }

        private async UniTask DisableCollisionTask(Func<bool> condition)
        {
            var colliders = gameObject.GetComponents<Collider2D>();

            var dictionary = new Dictionary<Collider2D, bool>();
            
            foreach (var c in colliders)
            {
                dictionary.Add(c, c.enabled);
                c.enabled = false;
            }

            await UniTask.WaitUntil(condition);
            
            foreach (var c in colliders)
            {
                c.enabled = dictionary[c];
            }
        }

        private IEnumerator PauseDamageForSeconds(float seconds)
        {
            _damageOnCooldown = true;
            yield return new WaitForSeconds(seconds);
            _damageOnCooldown = false;
        }

        public void UsernameHook(string oldUser, string newUser)
        {
            OnUsernameChange?.Invoke(newUser);
        }
        
        [GameCommand (Keyword = "nickname", PermissionLevel = PermissionLevel.Default, SuccessMessage = "Name changed.")]
        public static void SetUsername(string newUser)
        {
            LocalInstance.username = newUser;
        }
        
        public static Action<NetworkIdentity> LocalPlayerChangeIdentity;

        public override void OnStartServer()
        {
            base.OnStartServer();

            OnParentIdentityChange += ParentIdChangeHookServer;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            
            OnParentIdentityChange -= ParentIdChangeHookServer;
        }

        [Server]
        private void ParentIdChangeHookServer(NetworkIdentity old, NetworkIdentity newId)
        {
            if (old != null)
            {
                var oldRoom = RoomManager.Instance.GetRoomFromID(old.netId);
                if (oldRoom != null) oldRoom.CmdSetPlayerCount(oldRoom.PlayerCount - 1);
            }

            if (newId != null)
            {
                var newRoom = RoomManager.Instance.GetRoomFromID(newId.netId);
                if (newRoom != null) newRoom.CmdSetPlayerCount(newRoom.PlayerCount + 1);
            }
        }

        public override void OnStartLocalPlayer()
        {
            Team = Team.Player;
            LocalInstance = this;
            OnLocalPlayerCreated?.Invoke();

            OnParentIdentityChange += (old, newId) => LocalPlayerChangeIdentity?.Invoke(newId);

            OnParentIdentityChange += (old, newId) => { LocalInstance.StartCoroutine(PauseDamageForSeconds(1f)); };

            OnAttack += () =>
            {
                var projectileData = PlayerInventory.Instance.WeaponItem.projectiles;

                if (projectileData == null) return;

                foreach (var proj in projectileData)
                {
                    var pos = transform.position;

                    ObjectSpawner.SpawnProjectile(this, (Vector2) pos - new Vector2(0, 0.4f),
                        GameManager.MouseDirectionLow, proj);
                }
            };

            OnDamageOther += (dmg, ge, crit, dt) =>
            {
                if (dmg <= 0) return;
                if (ge != null)
                {
                    var percentDealt = dmg / ge.MaxHealth;
                    var percentMissing = 1 - ge.CurrentHealth / ge.MaxHealth;
                    SmoothCameraFollow.Shake(Mathf.Log(percentDealt * (1 + percentMissing / 5f) * (crit ? 32f : 24f)),
                        crit ? 2f : 1.5f);
                    Instantiate(ObjectFinder.GetPrefab("hitParticles"), ge.transform.position, Quaternion.identity);
                }
                else
                {
                    SmoothCameraFollow.Shake(3f, 1.5f);
                }
            };

            OnHurtOwner += (dmg) =>
            {
                var dmgPercent = dmg / Stats.GetStat(Stat.Health);
                if (dmgPercent < 0.1f) return;
                SmoothCameraFollow.Shake(dmgPercent * 1f, 100f);
                //SmoothCameraFollow.Shake(dmg/Stats.GetStat(Stat.Health), 5f);
            };

            //OnDamageOther += (_, _, _) => SoundManager.PlaySoundPitchVariant("hit", transform.position, 0.7f, 1f);

            SetUsername(CustomRoomPlayer.Local.username);

            UsernameHook("", CustomRoomPlayer.Local.username);

            AddEffect(new PercentRegenEffect
            {
                PercentRegen = 0.01f
            }, this);

            var fxString = CustomRoomPlayer.Local.classEffectName;
            
            if(!string.IsNullOrEmpty(fxString)) AddEffect(GameEffect.CreateEffect(fxString), this);

            PlayerInventory.Instance.InsertStarterItems();

            TransitionController.Instance.FadeIn();
            
            //CmdSetHealth(MaxHealth, MaxHealth);
        }

        [GameCommand(Keyword = "god", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Toggled invulnerability.")]
        public static void ToggleInvulnerable()
        {
            LocalInstance.SetInvulnerable(!LocalInstance.IsInvulnerable);
        }
    }
}