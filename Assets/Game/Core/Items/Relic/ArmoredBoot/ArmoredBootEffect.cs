using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class ArmoredBootEffect : GameEffect
    {
        public float armorBonusFlat = 20f;
        private float ArmorBonusFlatFinal => armorBonusFlat + 10f * playerStrengthFactorUponCreation;
        public float armorBonusMultiplier = 0.5f;
        private float ArmorBonusMultFinal => armorBonusMultiplier + 0.1f * playerStrengthFactorUponCreation;
        public float moveSpeedPenalty = 0.2f;

        public override void OnAddOwner()
        {
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.Armor, ArmorBonusFlatFinal, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Armor, ArmorBonusMultFinal, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, -moveSpeedPenalty, "ArmoredBoot");
        }

        public override void OnRemoveOwner()
        {
            PlayerStats.Stats.RemoveStatBonuses("ArmoredBoot");
        }

        public override void OnUpdateOwner()
        {
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>Grants {Tools.WrapColor($"{ArmorBonusFlatFinal:N0}", Color.yellow)} armor and increases player armor by {Tools.WrapColor($"{ArmorBonusMultFinal:P0}", Color.yellow)}.</color>\n<color=red>Lose {Tools.WrapColor($"{moveSpeedPenalty:P0}", Color.yellow)} of movement speed.</color>";
        }
    }
}