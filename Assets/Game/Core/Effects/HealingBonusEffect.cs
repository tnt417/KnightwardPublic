using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class HealingBonusEffect : GameEffect
    {
        public float Factor;
        
        public override void OnAddOwner()
        {
            Entity.HealMultiplier += Factor;
        }

        public override void OnRemoveOwner()
        {
            Entity.HealMultiplier -= Factor;
        }
    }
}