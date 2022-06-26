using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Items
{
    public enum ItemType
    {
        Weapon, Armor, Relic
    }

    public enum ItemRarity
    {
        Common, Uncommon, Rare, Unique
    }

    public class Item
    {
        public Sprite UISprite;
        public string ItemName;
        public ItemType ItemType;
        public ItemRarity ItemRarity;
        public StatBonus[] StatBonuses;
    }
}