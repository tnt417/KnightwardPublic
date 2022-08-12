using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "armoredBootEffect")]
    public class ArmoredBootEffect : GameEffect
    {
        public float armorBonusFlat = 20f;
        public float armorBonusMultiplier = 0.5f;
        public float moveSpeedPenalty = 0.2f;

        public override void OnAdd(GameEntity source)
        {
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.Armor, armorBonusFlat, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Armor, armorBonusMultiplier, "ArmoredBoot");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, -moveSpeedPenalty, "ArmoredBoot");
        }

        public override void OnRemove()
        {
            PlayerStats.Stats.RemoveStatBonuses("ArmoredBoot");
        }

        public override void OnUpdate()
        {
        }
    }
}