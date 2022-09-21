using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    public class ArmoredBootEffect : GameEffect
    {
        public float armorBonusFlat = 20f;
        public float armorBonusMultiplier = 0.5f;
        public float moveSpeedPenalty = 0.2f;

        public override void OnAddOwner()
        {
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.Armor, armorBonusFlat, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Armor, armorBonusMultiplier, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, -moveSpeedPenalty, "ArmoredBoot");
        }

        public override void OnRemoveOwner()
        {
            PlayerStats.Stats.RemoveStatBonuses("ArmoredBoot");
        }

        public override void OnUpdateOwner()
        {
        }
    }
}