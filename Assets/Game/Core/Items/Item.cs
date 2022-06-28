using System;
using TonyDev.Game.Core.Entities.Player;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Items
{
    public enum ItemType
    {
        Weapon, Armor, Relic, Tower
    }

    public enum ItemRarity
    {
        Common, Uncommon, Rare, Unique
    }

    [Serializable]
    public class Item
    {
        public static ItemType RandomItemType => (ItemType) Random.Range(0, 3); //A random item type
        public bool IsEquippable => itemType is ItemType.Armor or ItemType.Relic or ItemType.Weapon;
        public bool IsSpawnable => itemType is ItemType.Tower;
        
        //Editor variables
        public Sprite uiSprite;
        public string itemName;
        public ItemType itemType;
        public ItemRarity itemRarity;
        public StatBonus[] statBonuses;
        public GameObject spawnablePrefab;
        //

        public static ItemRarity RandomRarity(int rarityBoost)
        {
            //Roll for a random rarity
            var rarityRoll = Random.Range(0, 100) + rarityBoost;
            var itemRarity = rarityRoll switch
            {
                var n when n >= 95 => ItemRarity.Unique, //5% chance for unique
                var n when n >= 75 => ItemRarity.Rare, //20% chance for rare
                var n when n >= 40 => ItemRarity.Uncommon, //35% chance for uncommon
                var n when n >= 0 => ItemRarity.Common, //40% chance for common
                _ => ItemRarity.Common
            };
            //
            
            return itemRarity;
        }
    }
}