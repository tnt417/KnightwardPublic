using System.Collections.Generic;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class DaggerEffect : AbilityEffect
    {
        public override void OnAddOwner()
        {
            ActivateButton = KeyCode.None;
            base.OnAddOwner();
            Entity.OnAttack += PlaySound;
            
            Player.LocalInstance.playerAnimator.SetWeaponAnimSprite("dagger_anim");
            
            Player.LocalInstance.playerAnimator.attackAnimationName = "Attack";
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
            Entity.OnAttack -= PlaySound;
        }

        private void PlaySound()
        {
            SmoothCameraFollow.Shake(1, 3f);
            SoundManager.PlaySoundPitchVariant("dagger",0.5f, Entity.transform.position, 1.4f, 1.6f);
        }

        protected override void OnAbilityActivate()
        {
            base.OnAbilityActivate();
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Dodge, 0.5f, "daggerEffect"), Duration);
            SoundManager.PlaySound("woosh", 0.5f, Entity.transform.position, Random.Range(1.1f, 1.2f));
            Player.LocalInstance.playerMovement.Dash(3f, Vector2.zero);
        }

        protected override void OnAbilityDeactivate()
        {
            Entity.Stats.RemoveBuffs(new HashSet<string> {"daggerEffect"});
        }
    }
}