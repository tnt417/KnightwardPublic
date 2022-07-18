using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    [CreateAssetMenu(menuName = "Item Effects/Omni Jewel Effect")]
    public class OmniJewelEffect : ItemEffect
    {
        public float multiplyBonus = 0.1f;
        public override void OnAdd()
        {
            PlayerStats.AddStatBonus(StatType.Multiplicative, Stat.Damage, multiplyBonus, "OmniJewel");
            PlayerStats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, multiplyBonus, "OmniJewel");
            PlayerStats.AddStatBonus(StatType.Multiplicative, Stat.Armor, multiplyBonus, "OmniJewel");
            PlayerStats.AddStatBonus(StatType.Multiplicative, Stat.Health, multiplyBonus, "OmniJewel");
        }

        public override void OnRemove()
        {
            PlayerStats.RemoveStatBonuses("OmniJewel");
        }

        public override void OnUpdate()
        {
        }
    }
}
