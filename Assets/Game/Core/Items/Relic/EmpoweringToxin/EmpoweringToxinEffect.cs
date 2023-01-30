using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.EmpoweringToxin
{
    public class EmpoweringToxinEffect : AbilityEffect
    {
        private float MoveSpeedStrength => LinearScale(2, 5, 50);
        private float CritChanceStrength => DiminishingScale(0.3f, 0.5f, 50);
        private float AttackSpeedStrength => DiminishingScale(0.4f, 0.75f, 50);

        protected override void OnAbilityActivate()
        {
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.Flat, Stat.MoveSpeed, MoveSpeedStrength, "EmpoweringToxin"),
                Duration);
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.Flat, Stat.CritChance, CritChanceStrength, "EmpoweringToxin"), Duration);
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, AttackSpeedStrength, "EmpoweringToxin"),
                Duration);
        }

        protected override void OnAbilityDeactivate()
        {
        }

        public override string GetEffectDescription()
        {
            return $"<color=green>Upon activation, gain {GameTools.WrapColor($"{MoveSpeedStrength:P0}", Color.yellow)} " +
                   $"move speed, {GameTools.WrapColor($"{CritChanceStrength:P0}", Color.yellow)} crit chance, and {GameTools.WrapColor($"{AttackSpeedStrength:P0}", Color.yellow)} attack speed for {GameTools.WrapColor($"{Duration} seconds", Color.white)}. {GameTools.WrapColor($"{Cooldown} second", Color.white)} cooldown.</color>";
        }
    }
}