using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.EmpoweringToxin
{
    [GameEffect(ID="empoweringToxinEffect")]
    public class EmpoweringToxinEffect : AbilityEffect
    {
        public override void OnAdd(GameEntity source)
        {
            Cooldown = 10;
            Duration = 5;
            ActivateButton = KeyCode.E;
        }

        public override void OnRemove()
        {
        }

        protected override void OnAbilityActivate()
        {
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.MoveSpeed, 1, "EmpoweringToxin"), Duration);
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.CritChance, 0.5f, "EmpoweringToxin"), Duration);
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, 1, "EmpoweringToxin"), Duration);
        }

        protected override void OnAbilityDeactivate()
        {
        }
    }
}
