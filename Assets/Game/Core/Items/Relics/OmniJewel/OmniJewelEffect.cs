using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "omniJewelEffect")]
    public class OmniJewelEffect : GameEffect
    {
        public float additivePercentBonus = 0.1f;
        public override void OnAdd(GameEntity source)
        {
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, additivePercentBonus, "OmniJewel");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.AttackSpeed, additivePercentBonus, "OmniJewel");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Armor, additivePercentBonus, "OmniJewel");
            PlayerStats.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, additivePercentBonus, "OmniJewel");
        }

        public override void OnRemove()
        {
            PlayerStats.Stats.RemoveStatBonuses("OmniJewel");
        }

        public override void OnUpdate()
        {
        }
    }
}
