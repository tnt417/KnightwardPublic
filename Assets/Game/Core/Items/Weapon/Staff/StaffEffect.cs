using System;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEditor.SceneTemplate;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Items.Weapon.Staff
{
    public class StaffEffect : AbilityEffect
    {
        private GameObject _staffObject;
        private LaserWeaponController _controller;

        public override void OnAddClient()
        {
            _staffObject = GameObject.Instantiate(ObjectFinder.GetPrefab("laserController"));
            _controller = _staffObject.GetComponent<LaserWeaponController>();
            _controller.IsOwner = Entity.isLocalPlayer;
        }
        
        public override void OnAddOwner()
        {
            Entity.OnAttack += OnAttack;
            Player.LocalInstance.playerAnimator.attackAnimationName = "Overhead";
        }

        public override void OnUpdateOwner()
        {

        }

        private void OnAttack()
        {
            
        }

        private void PlaySound()
        {
            SoundManager.PlaySound("dagger", 0.5f, Entity.transform.position, Random.Range(0.6f, 0.8f));
        }

        public override void OnRemoveOwner()
        {
            Entity.OnAttack -= OnAttack;
        }

        public override void OnRemoveClient()
        {
            GameObject.Destroy(_staffObject);
        }
    }
}