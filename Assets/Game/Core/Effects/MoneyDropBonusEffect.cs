using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class MoneyDropBonusEffect : GameEffect
    {
        public float moneyDropFactorBonus;
        
        public override void OnAddOwner()
        {
            if(Entity is Player) GameManager.MoneyDropBonusFactor += moneyDropFactorBonus;
        }

        public override void OnRemoveOwner()
        {
            if(Entity is Player) GameManager.MoneyDropBonusFactor -= moneyDropFactorBonus;
        }
    }
}