using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    [CreateAssetMenu(menuName = "Item Effects/Armored Boot Effect")]
    public class ArmoredBootEffect : ItemEffect
    {
        public float armorBonusMultiplier = 0.5f;
        public float moveSpeedPenalty = 0.2f;

        public override void OnAdd()
        {
            PlayerStats.AddStatBonus(StatType.Flat, Stat.Armor, armorBonusMultiplier, "ArmoredBoot");
            PlayerStats.AddStatBonus(StatType.Multiplicative, Stat.MoveSpeed, -moveSpeedPenalty, "ArmoredBoot");
        }

        public override void OnRemove()
        {
            PlayerStats.RemoveStatBonuses("ArmoredBoot");
        }

        public override void OnUpdate()
        {
        }
    }
}