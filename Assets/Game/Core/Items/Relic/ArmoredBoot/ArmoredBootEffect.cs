using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class ArmoredBootEffect : GameEffect
    {
        public int armorFlatBase = 20;
        private float ArmorBonusFlatFinal => LinearScale(armorFlatBase, armorFlatBase+50, 50);
        public Vector2 armorMultScale;
        private float ArmorBonusMultFinal => DiminishingScale(armorMultScale.x, armorMultScale.y, 50);
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
                $"<color=#63ab3f>Grants {GameTools.WrapColor($"{ArmorBonusFlatFinal:N0}", Color.yellow)} armor and increases player armor by {GameTools.WrapColor($"{ArmorBonusMultFinal:P0}", Color.yellow)}.</color>\n<color=red>Lose {GameTools.WrapColor($"{moveSpeedPenalty:P0}", Color.yellow)} of movement speed.</color>";
        }
    }
}