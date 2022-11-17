using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relic.CrystalFragmentNecklace
{
    public class CrystalNecklaceEffect : GameEffect
    {
        private CrystalNecklaceEffect _crystalNecklaceEffect;

        public float ArmorBonusFlat;
        private float ArmorBonusFlatFinal => ArmorBonusFlat * PlayerStrengthFactorUponCreation;
        
        public override void OnAddOwner() //Runs when an item is equipped
        {
            if (Entity is not Crystal)
            {
                Entity.Stats.AddStatBonus(StatType.Flat, Stat.Armor, ArmorBonusFlatFinal, "CrystalNecklace"); //Give the player 50 armor
                
                _crystalNecklaceEffect = new CrystalNecklaceEffect();
                Crystal.Instance.CmdAddEffect(_crystalNecklaceEffect, Entity);
            }
        }

        public override void OnRemoveOwner() //Runs when an item is unequipped
        {
            Entity.Stats.RemoveStatBonuses("CrystalNecklace");
            if(Entity is not Crystal) Crystal.Instance.CmdRemoveEffect(_crystalNecklaceEffect);
        }

        private double _nextBuffTime;

        public override void OnUpdateOwner()
        {
            if (Time.time > _nextBuffTime && Entity is Crystal)
            {
                _nextBuffTime = Time.time + 1f;
                Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Armor, Source.Stats.GetStat(Stat.Armor), "CrystalNecklace"), 1f); //Give the crystal the player's armor bonus
            }
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>Gain {Tools.WrapColor($"{ArmorBonusFlatFinal:N0}", Color.yellow)} armor. The crystal is affected by your total armor bonus.</color>";
        }
    }
}
