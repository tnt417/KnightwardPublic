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

        public override void OnAddOwner()
        {
            ActivateButton = KeyCode.Q;
        }

        public override void OnRemoveOwner()
        {
            OnAbilityDeactivate();
        }

        private PoisonEffect _poisonEffect;
        
        protected override void OnAbilityActivate()
        {
            Debug.Log(nameof(PoisonEffect));
            _poisonEffect = new PoisonEffect()
            {
                Damage = PoisonDPSMultiplier * Entity.DamageMultiplier,
                Ticks = 40,
                Frequency = 0.25f
            };
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Add(_poisonEffect);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, MoveSpeedMultiplier, "spearEffect"), Duration);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, AttackSpeedMultiplier, "spearEffect"), Duration);
            Cooldown -= 1f;
        }

        protected override void OnAbilityDeactivate()
        {
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Remove(_poisonEffect);
        }
    }
}
