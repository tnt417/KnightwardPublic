using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class HealingBonusEffect : GameEffect
    {
        public Vector2 BonusScale;
        
        private float FactorFinal => LinearScale(BonusScale.x, BonusScale.y, 50);
        
        public override void OnAddOwner()
        {
            Entity.HealMultiplier += FactorFinal;
        }

        public override void OnRemoveOwner()
        {
            Entity.HealMultiplier -= FactorFinal;
        }

        public override string GetEffectDescription()
        {
            return $"<color=green>{GameTools.WrapColor($"+{FactorFinal:P0}", Color.yellow)} to all incoming healing.</color>";
        }
    }
}