using System;
using System.Collections.Generic;
using System.Text;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items.ItemEffects;
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
        public static ItemType RandomItemType
        {
            get
            {
                var roll = Random.Range(0, 100);
                return roll switch
                {
                    >= 70 => ItemType.Armor,
                    >= 40 => ItemType.Relic,
                    >= 10 => ItemType.Weapon,
                    >= 0 => ItemType.Tower,
                    _ => throw new ArgumentOutOfRangeException()
                };
                //A random item type. Towers are twice as rare as standard item types.
            }
        }

        public bool IsEquippable => itemType is ItemType.Armor or ItemType.Relic or ItemType.Weapon;
        public bool IsSpawnable => itemType is ItemType.Tower;
        
        //Editor variables
        public Sprite uiSprite;
        public string itemName;
        [TextArea(3,3)]
        public string itemDescription;
        public ItemType itemType;
        public ItemRarity itemRarity;
        public StatBonus[] statBonuses;
        public GameObject spawnablePrefab;
        public ItemEffect[] itemEffects;
        public ProjectileData[] projectiles;
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
        
        public string GetItemDescription() //Returns a string that contains a specified item's name and stats, all on their own line
        {
            var stringBuilder = new StringBuilder();
            
            if(itemDescription != string.Empty) stringBuilder.AppendLine(itemDescription);

            if(IsEquippable) stringBuilder.AppendLine("<color=grey>" + PlayerStats.GetStatsText(statBonuses) + "</color>");

            return stringBuilder.ToString(); //Return the string
        }
    }
}