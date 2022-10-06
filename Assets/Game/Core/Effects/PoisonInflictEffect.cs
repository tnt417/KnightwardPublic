using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PoisonInflictEffect : GameEffect
    {
        public float TotalDamageMultiplier;
        public float TickFrequency;
        public int TickAmount;
        
        private PoisonEffect _poisonEffect;
        public override void OnAddOwner()
        {
            _poisonEffect = new PoisonEffect()
            {
                Damage = TotalDamageMultiplier * Entity.Stats.GetStat(Stat.Damage) / TickAmount,
                Ticks = TickAmount,
                Frequency = TickFrequency
            };
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Add(_poisonEffect);
        }

        public override void OnRemoveOwner()
        {
            PlayerInventory.Instance.WeaponItem.projectiles[0].effects.Remove(_poisonEffect);
        }
    }
}