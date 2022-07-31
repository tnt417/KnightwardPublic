using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

//Provides functionality for randomly generating items
namespace TonyDev.Game.Core.Items
{
    public static class ItemGenerator
    {
        private static Sprite[] _weaponSprites; //Possible weapon sprites to pick from
        private static Sprite[] _armorSprites; //Possible armor sprites to pick from
        private static Sprite[] _relicSprites; //Possible relic sprites to pick from

        public static void
            InitSprites() //Load sprites from SpriteDictionary based on the prefix to be used in randomly generated items.
        {
            _weaponSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("wi_")).Select(s => s.Value)
                .ToArray();
            _armorSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("ai_")).Select(s => s.Value).ToArray();
            _relicSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("ri_")).Select(s => s.Value).ToArray();
        }

        private static Item RandomCustomItem => Tools.SelectRandom(GameManager.AllItems.ToArray());
        //private static Item RandomTowerItem => Tools.SelectRandom(GameManager.AllItems.Where(i => i.itemType == ItemType.Tower).ToArray());

        private static Item GetTowerItem(ItemRarity itemRarity)
        {
            return Tools.SelectRandom(GameManager.AllItems.Where(i => i.itemType == ItemType.Tower && i.itemRarity == itemRarity).ToArray());
        }
        
        public static Item GenerateItem(int rarityBoost)
        {
            var itemType = Item.RandomItemType;

            var item = itemType switch
            {
                ItemType.Weapon or ItemType.Armor or ItemType.Relic or ItemType.Tower => GenerateItemOfType(
                    itemType, Item.RandomRarity(rarityBoost)),
                _ => null
            };

            return item;
        }
        
        //Returns a randomly generated item based on input parameters
        public static Item GenerateItemOfType(ItemType type, ItemRarity rarity)
        {
            //Instantiate local variables that will end up at the parameters for the generated item.
            Item item = null;
            var itemName = string.Empty;
            Sprite sprite = null;
            //
            
            
            if (type == ItemType.Weapon)
            {
                item = Tools.SelectRandom(GameManager.AllItems.Where(i => i.itemType == ItemType.Weapon));
            }

            //Set sprites and item names based on the item type
            switch (type)
            {
                case ItemType.Weapon:
                    if (item == null) return null;
                    sprite = item.uiSprite;
                    itemName = item.itemName;
                    break;
                case ItemType.Armor:
                    sprite = Tools.SelectRandom(_armorSprites);
                    itemName = "Armor";
                    break;
                case ItemType.Relic:
                    return Tools.SelectRandom(GameManager.AllItems.Where(i => i.itemType == ItemType.Relic));
                case ItemType.Tower:
                    return GetTowerItem(rarity);
            }

            //Return a new item using all the variables this function has generated and/or been provided with
            return new Item()
            {
                itemType = type,
                itemRarity = rarity,
                itemName = itemName,
                statBonuses = item == null ? GenerateItemStats(type, rarity) : StatBonus.Combine(GenerateItemStats(type, rarity), item.statBonuses).ToArray(),
                uiSprite = sprite,
                ItemEffects = item?.ItemEffects
            };
        }

        #region Stat Generation

        private static float StatStrengthFactor => 1 + GameManager.DungeonFloor/15f;
        private static float DamageStrength => Random.Range(0.6f, 1f) * StatStrengthFactor * 25f;
        private static float AttackSpeedStrength => Random.Range(0.6f, 1f) * StatStrengthFactor;
        private static float ArmorStrength => Random.Range(0.6f, 1f) * 10 * StatStrengthFactor;
        private static float HealthStrength => Random.Range(0.6f, 1f) * 10 * StatStrengthFactor;
        private static StatBonus[] GenerateItemStats(ItemType itemType, ItemRarity itemRarity)
        {
            if (itemType is not (ItemType.Armor or ItemType.Weapon)) return null;

            /*Weapons:
            Common = base dmg, base attack spd
            Uncommon = base dmg+, base attack spd+
            Rare = base dmg++, base attack spd++, bonus stat
            Unique = base dmg++, base attack spd++, bonus stat, bonus stat, special effect (ONLY DROPS FROM SPECIAL PLACES!) TODO*/
            
            //1. add stat bonuses to an array without a strength
            //2. adjust the strength of the effects depending on the stat

            var statBonuses = new List<StatBonus>();

            var multiplier = itemRarity switch
            {
                ItemRarity.Common => 1f,
                ItemRarity.Uncommon => 1.1f,
                ItemRarity.Rare => 1.2f,
                ItemRarity.Unique => 1.2f,
                _ => 1f
            };

            var source = Enum.GetName(typeof(ItemType), itemType);
            
            switch (itemType)
            {
                case ItemType.Weapon:
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.Damage, DamageStrength * multiplier, source));
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.AttackSpeed, AttackSpeedStrength * multiplier, source));
                    break;
                case ItemType.Armor:
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.Armor, ArmorStrength * multiplier, source));
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.Health, HealthStrength * multiplier, source));
                    break;
            }

            switch (itemRarity)
            {
                case ItemRarity.Rare:
                    var stat1 = PlayerStats.GetValidStatForItem(itemType);
                    statBonuses.Add(new StatBonus(StatType.Flat, stat1, GetBonusStatStrength(stat1), source));
                    break;
                case ItemRarity.Unique:
                    stat1 = PlayerStats.GetValidStatForItem(itemType);
                    statBonuses.Add(new StatBonus(StatType.Flat, stat1, GetBonusStatStrength(stat1), source));
                    var stat2 = PlayerStats.GetValidStatForItem(itemType);
                    statBonuses.Add(new StatBonus(StatType.Flat, stat2, GetBonusStatStrength(stat2), source));
                    break;
            }

            return statBonuses.ToArray();
        }

        private static float GetBonusStatStrength(Stat stat)
        {
            return stat switch
            {
                Stat.Damage => DamageStrength*1.2f,
                Stat.AttackSpeed => AttackSpeedStrength*0.2f,
                Stat.Armor => ArmorStrength * 0.2f,
                Stat.Health => HealthStrength * 0.2f,
                Stat.CritChance => Random.Range(0.2f, 0.4f) + Mathf.Clamp01(1000 / (-1 * Mathf.Pow((StatStrengthFactor-1)*5, 2)) + 0.5f),
                //Stat.CritDamage => DamageStrength*Mathf.Clamp01(StatStrengthFactor-1),
                Stat.Dodge => Random.Range(0.2f, 0.4f) + Mathf.Clamp01(1000 / (-1 * Mathf.Pow((StatStrengthFactor-1)*5, 2)) + 0.5f),
                //Stat.Knockback => Random.Range(0.1f, 0.5f),
                //Stat.Stun => 0,
                //Stat.AoeSize => Random.Range(0.2f, 0.75f),
                Stat.MoveSpeed => Random.Range(0.1f, 0.5f),
                //Stat.Tenacity => 0, //Random.Range(0.2f, 0.5f) + Mathf.Clamp01(1000 / (-1 * Mathf.Pow((StatStrengthFactor-1)*5, 2)) + 0.5f),
                //Stat.HpRegen => HealthStrength / 100,
                //Stat.DamageReduction => Random.Range(0.1f, 0.3f) + Mathf.Clamp01(1000 / (-1 * Mathf.Pow((StatStrengthFactor-1)*5, 2)) + 0.5f),
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
        #endregion

        public static int GenerateCost(Item item)
        {
            return (int) (item.itemRarity switch
            {
                ItemRarity.Common => 10f * (1 + GameManager.DungeonFloor / 5f),
                ItemRarity.Uncommon => 15f * (1 + GameManager.DungeonFloor / 5f),
                ItemRarity.Rare => 20f * (1 + GameManager.DungeonFloor / 5f),
                ItemRarity.Unique => 25f * (1 + GameManager.DungeonFloor / 5f),
                _ => 0
            });
        }
    }
}