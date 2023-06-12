using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class DaggerEffect : AbilityEffect
    {
        public override void OnAddOwner()
        {
            base.OnAddOwner();
            Entity.OnAttack += PlaySound;
            
            Player.LocalInstance.playerAnimator.attackAnimationName = "Attack";
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
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Dodge, 0.5f, "daggerEffect"), Duration);
            SoundManager.PlaySound("woosh", 0.5f, Entity.transform.position, Random.Range(0.7f, 0.8f));
            Player.LocalInstance.playerMovement.Dash(3f, Vector2.zero);
        }

        protected override void OnAbilityDeactivate()
        {
            Entity.Stats.RemoveBuffs(new[] {"daggerEffect"});
        }
    }
}