using System;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Items.Weapon.Staff
{
    public class StaffEffect : AbilityEffect
    {
        private GameObject _staffObject;
        private LaserWeaponController _controller;

        public override void OnAddClient()
        {
            base.OnAddClient();
            _staffObject = GameObject.Instantiate(ObjectFinder.GetPrefab("laserController"));
            _controller = _staffObject.GetComponent<LaserWeaponController>();
            _controller.IsOwner = Entity.isLocalPlayer;
        }
        
        public override void OnAddOwner()
        {
            ActivateButton = KeyCode.None;
            base.OnAddOwner();
            Entity.OnAttack += OnAttack;
            Player.LocalInstance.playerAnimator.attackAnimationName = "Overhead";
        }

        protected override void OnAbilityActivate()
        {
            base.OnAbilityActivate();

            GameObject.Instantiate(ObjectFinder.GetPrefab("staffMirror"), (Vector2)GameManager.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Quaternion.identity);
        }

        private void OnAttack()
        {
            
        }

        private void PlaySound()
        {
            SoundManager.PlaySound("dagger", 0.5f, Entity.transform.position, Random.Range(0.6f, 0.8f));
        }

        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();
            
            if (Mouse.current.rightButton.isPressed && Ready)
            {
                OnAbilityActivate();
            }
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            Entity.OnAttack -= OnAttack;
            Player.LocalInstance.playerAnimator.Shake(0);
        }

        public override void OnRemoveClient()
        {
            base.OnRemoveClient();
            GameObject.Destroy(_staffObject);
        }
    }
}