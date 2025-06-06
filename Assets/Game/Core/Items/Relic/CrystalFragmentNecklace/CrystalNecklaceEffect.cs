using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relic.CrystalFragmentNecklace
{
    public class CrystalNecklaceEffect : GameEffect
    {

        public float ArmorBonusFlat;
        private float ArmorBonusFlatFinal => LinearScale(ArmorBonusFlat, ArmorBonusFlat+50, 50);
        
        public override void OnAddServer()
        {
            //TODO: unsure if OnStatChanged gets called when receiving new stats for a non-owned GameEntity, if not this won't work
            Entity.Stats.OnStatChanged += (statChanged) =>
            {
                if (statChanged != Stat.Armor) return;
                UpdateArmorBonus();
            };
            
            UpdateArmorBonus();
        }

        public override void OnAddOwner()
        {
            Entity.Stats.AddStatBonus(StatType.Flat, Stat.Armor, ArmorBonusFlatFinal, EffectIdentifier); //Give the player the armor bonus
        }

        private void UpdateArmorBonus()
        {
            Crystal.Instance.Stats.RemoveStatBonuses(EffectIdentifier);
            Crystal.Instance.Stats.AddStatBonus(StatType.Flat, Stat.Armor, Entity.Stats.GetStat(Stat.Armor), EffectIdentifier);
        }

        public override void OnRemoveServer() //Runs when an item is unequipped
        {
            Crystal.Instance.Stats.RemoveStatBonuses(EffectIdentifier);
            
            Entity.Stats.OnStatChanged -= (statChanged) =>
            {
                if (statChanged != Stat.Armor) return;
                UpdateArmorBonus();
            };
        }
        
        public override void OnRemoveOwner() //Runs when an item is unequipped
        {
            Entity.Stats.RemoveStatBonuses(EffectIdentifier);
        }

        private double _nextBuffTime;

        public override string GetEffectDescription()
        {
            return $"<color=#63ab3f>Gain {GameTools.WrapColor($"{ArmorBonusFlatFinal:N0}", Color.yellow)} armor. The crystal is affected by your total armor bonus.</color>";
        }
    }
}
