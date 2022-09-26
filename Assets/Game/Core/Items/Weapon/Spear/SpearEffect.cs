using System;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [Serializable]
    public class SpearEffect : AbilityEffect
    {
        public float MoveSpeedMultiplier;
        public float AttackSpeedMultiplier;
        public float PoisonDPSMultiplier;

        private PoisonEffect _poisonEffect;
        
        protected override void OnAbilityActivate()
        {
            _poisonEffect = new PoisonEffect()
            {
                Damage = PoisonDPSMultiplier * Entity.Stats.GetStat(Stat.Damage),
                Ticks = 10,
                Frequency = 1f
            };
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Add(_poisonEffect);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, MoveSpeedMultiplier, "spearEffect"), Duration);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, AttackSpeedMultiplier, "spearEffect"), Duration);
        }

        protected override void OnAbilityDeactivate()
        {
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Remove(_poisonEffect);
        }
    }
}
