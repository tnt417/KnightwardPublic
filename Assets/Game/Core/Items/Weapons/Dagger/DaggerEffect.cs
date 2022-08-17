using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "daggerEffect")]
    public class DaggerEffect : AbilityEffect
    {
        public override void OnAdd(GameEntity source)
        {
            Cooldown = 5f;
            Duration = 2.5f;
            ActivateButton = KeyCode.Q;
        }

        public override void OnRemove()
        {
            OnAbilityDeactivate();
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