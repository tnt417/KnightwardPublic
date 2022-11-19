using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class HealingBonusEffect : GameEffect
    {
        public float Factor;
        
        private float FactorFinal => Factor + 0.1f * playerStrengthFactorUponCreation;
        
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
            return $"<color=green>{Tools.WrapColor($"+{FactorFinal:P0}", Color.yellow)} to all incoming healing.</color>";
        }
    }
}