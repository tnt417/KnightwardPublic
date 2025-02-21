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
            return $"<color=#63ab3f>{GameTools.WrapColor($"+{FactorFinal:P0}", Color.yellow)} to all incoming healing.</color>";
        }
    }
}