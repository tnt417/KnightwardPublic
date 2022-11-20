using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class MoneyDropBonusEffect : GameEffect
    {
        public Vector2 MoneyDropBonusScale;
        
        private float MoneyBonusFinal => LinearScale(MoneyDropBonusScale.x, MoneyDropBonusScale.y, 50);
        
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