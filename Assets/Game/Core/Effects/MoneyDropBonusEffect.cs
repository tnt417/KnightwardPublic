using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class MoneyDropBonusEffect : GameEffect
    {
        public float moneyDropFactorBonus;
        
        private float MoneyBonusFinal => moneyDropFactorBonus * 0.8f * playerStrengthFactorUponCreation;
        
        public override void OnAddOwner()
        {
            if(Entity is Player) GameManager.MoneyDropBonusFactor += MoneyBonusFinal;
        }

        public override void OnRemoveOwner()
        {
            if(Entity is Player) GameManager.MoneyDropBonusFactor -= MoneyBonusFinal;
        }

        public override string GetEffectDescription()
        {
            return $"<color=green>Enemies and destructables drop {Tools.WrapColor($"{MoneyBonusFinal:P0}", Color.yellow)} more coins.";
        }
    }
}