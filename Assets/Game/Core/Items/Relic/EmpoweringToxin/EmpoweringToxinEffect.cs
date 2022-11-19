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
        private float MoveSpeedStrength => 0.7f * playerStrengthFactorUponCreation;
        private float CritChanceStrength => 0.4f * playerStrengthFactorUponCreation;
        private float AttackSpeedStrength => 0.7f * playerStrengthFactorUponCreation;

        protected override void OnAbilityActivate()
        {
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.AdditivePercent, Stat.MoveSpeed, MoveSpeedStrength, "EmpoweringToxin"),
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
            return $"<color=green>Upon activation, gain {Tools.WrapColor($"{MoveSpeedStrength:P0}", Color.yellow)} " +
                   $"move speed, {Tools.WrapColor($"{CritChanceStrength:P0}", Color.yellow)} crit chance, and {Tools.WrapColor($"{AttackSpeedStrength:P0}", Color.yellow)} attack speed for {Tools.WrapColor($"{Duration} seconds", Color.white)}. {Tools.WrapColor($"{Cooldown} second", Color.white)} cooldown.</color>";
        }
    }
}