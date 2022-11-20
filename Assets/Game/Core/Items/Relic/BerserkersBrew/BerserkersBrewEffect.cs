using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.BerserkersBrew
{
    public class BerserkersBrewEffect : AbilityEffect
    {
        private float RegenMultiplier => LinearScale(3, 6, 50);
        private float DamageMultiplier => LinearScale(0.8f, 1.5f, 50);
        protected override void OnAbilityActivate()
        {
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.HpRegen, RegenMultiplier, "BerserkersBrew"), Duration);
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, DamageMultiplier, "BerserkersBrew"), Duration);
        }

        protected override void OnAbilityDeactivate()
        {
        }

        public override string GetEffectDescription()
        {
            return $"<color=green>Upon activation, gain {Tools.WrapColor($"{RegenMultiplier:N1}x", Color.yellow)} " +
                   $"health regen and {Tools.WrapColor($"{DamageMultiplier:P0}", Color.yellow)} bonus damage for {Tools.WrapColor($"{Duration} seconds", Color.white)}. {Tools.WrapColor($"{Cooldown} second", Color.white)} cooldown.</color>";
        }
    }
}
