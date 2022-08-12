using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.BerserkersBrew
{
    [GameEffect(ID="berserkersBrewEffect")]
    public class BerserkersBrewEffect : AbilityEffect
    {
        public override void OnAdd(GameEntity source)
        {
            Cooldown = 20;
            Duration = 5;
            ActivateButton = KeyCode.E;
        }

        public override void OnRemove()
        {
        }

        protected override void OnAbilityActivate()
        {
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.HpRegen, 5, "BerserkersBrew"), Duration);
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, 1, "BerserkersBrew"), Duration);
        }

        protected override void OnAbilityDeactivate()
        {
        }
    }
}
