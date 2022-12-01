using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class DaggerEffect : AbilityEffect
    {
        public override void OnAddOwner()
        {
            base.OnAddOwner();
            Entity.OnAttack += PlaySound;
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            Entity.OnAttack -= PlaySound;
        }

        private void PlaySound()
        {
            SoundManager.PlaySoundPitchVariant("dagger",0.5f, Entity.transform.position, 0.8f, 1.2f);
        }

        protected override void OnAbilityActivate()
        {
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Dodge, 0.5f, "daggerEffect"), Duration);
            Player.LocalInstance.playerMovement.Dash(3f);
        }

        protected override void OnAbilityDeactivate()
        {
            Entity.Stats.RemoveBuffs(new[] {"daggerEffect"});
        }
    }
}