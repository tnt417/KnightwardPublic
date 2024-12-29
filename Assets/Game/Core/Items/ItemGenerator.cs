using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//Provides functionality for randomly generating items
namespace TonyDev.Game.Core.Items
{
    public static class ItemGenerator
    {
        private static Dictionary<string, int> _itemCounts = new();

        static ItemGenerator()
        {
            GameManager.OnGameManagerAwake += () => _itemCounts = new();
        }
        
        public static Item GenerateItem(int rarityBoost)
        {
            return GenerateItemOfType(Item.RandomItemType, Item.RandomRarity(rarityBoost));
        }

        public static Item GenerateItemFromData(ItemData data)
        {
            var item = Object.Instantiate(data).item;

            item.statBonuses = StatBonus
                .Combine(GenerateItemStats(item.itemType, item.itemRarity, item.bypassStatGeneration), item.statBonuses)
                .ToArray();

            if (item.itemType == ItemType.Relic)
            {
                return ScaleRelicStats(item);
            }

            if (_itemCounts.ContainsKey(item.itemName))
            {
                _itemCounts[item.itemName] += 1;
            }
            else
            {
                _itemCounts[item.itemName] = 0;
            }

            return item;
        }

        public static int GenerateSellPrice(Item item, int dungeonFloor = 0)
        {
            if (dungeonFloor <= 0) dungeonFloor = GameManager.DungeonFloor;

            if (item == null)
            {
                //Debug.LogWarning("Cannot generate essence for null item!");
                return 0;
            }

            return (int) Mathf.Pow(GenerateCost(item.itemRarity, dungeonFloor) * Random.Range(0.1f, 0.15f), 1.3f);
        }

        //Returns a randomly generated item based on input parameters
        public static Item GenerateItemOfType(ItemType type, ItemRarity rarity)
        {
            //Instantiate local variables that will end up at the parameters for the generated item.
            //

            var matchingItems = UnlocksManager.UnlockedItems.Where(i =>
                i.item.itemType == type && (i.item.itemRarity == rarity ||
                i.item.itemType is ItemType.Weapon)).ToArray();

            var filteredItemCounts = new Dictionary<string, int>(_itemCounts.Where(kv =>
                matchingItems.Any(i => i.item.itemName == kv.Key)));

            var validItems = matchingItems.Where(i =>
                !filteredItemCounts.ContainsKey(i.item.itemName) ||
                filteredItemCounts[i.item.itemName] < filteredItemCounts.Max(kv => kv.Value)).ToArray();
            
            if (!validItems.Any())
            {
                validItems = matchingItems.Where(i => filteredItemCounts.ContainsKey(i.item.itemName))
                    .ToArray();
            }

            var item = GameTools.SelectRandom(validItems.Select(id => Object.Instantiate(id).item));

            if (item == null)
            {
                //Debug.LogWarning("Failed to generate item! " + Enum.GetName(typeof(ItemType), type) + ", " +
                               //  Enum.GetName(typeof(ItemRarity), rarity));
                return null;
            }
            
            item.itemRarity = rarity;
            
            switch (type)
            {
                case ItemType.Weapon or ItemType.Tower:
                    item.statBonuses = StatBonus.Combine(GenerateItemStats(type, rarity, item.bypassStatGeneration),
                            item.statBonuses)
                        .ToArray();
                    //item.itemEffects = item?.itemEffects.AsReadOnly().ToList();
                    break;
                case ItemType.Relic:
                    item = ScaleRelicStats(item);
                    break;
            }

            if (_itemCounts.ContainsKey(item.itemName))
            {
                _itemCounts[item.itemName] += 1;
            }
            else
            {
                _itemCounts[item.itemName] = 0;
            }

            //Return a new item using all the variables this function has generated and/or been provided with
            return item;
        }

        #region Stat Generation

        public static float StatStrengthFactor => 1f + /*GameManager.DungeonFloor*/1 / 15f;
        private static float DamageStrength => /*Random.Range(0.95f, 1.05f) * /*GameManager.DungeonFloor*/25f;

        private static float AttackSpeedStrength =>
            Random.Range(0.95f, 1.05f); /* + Mathf.Log(GameManager.DungeonFloor, 50);*/

        private static float ArmorStrength => /*Random.Range(0.95f, 1.05f) */ 5 * /*GameManager.DungeonFloor*/1;
        private static float HealthStrength => /*Random.Range(0.95f, 1.05f) **/ 20 * /*GameManager.DungeonFloor*/1;

        private static Item ScaleRelicStats(Item original)
        {
            for (var i = 0; i < original.statBonuses.Length; i++)
            {
                var bonus = original.statBonuses[i];

                bonus.strength *= 1;//Mathf.Pow(0.8f + Mathf.Log(StatStrengthFactor), 2);

                original.statBonuses[i] = bonus;
            }

            return original;
        }

        private static StatBonus[] GenerateItemStats(ItemType itemType, ItemRarity itemRarity, bool bypassStatGen)
        {
            if (itemType is not (ItemType.Weapon or ItemType.Tower) || bypassStatGen) return null;

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
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.AttackSpeed, AttackSpeedStrength * multiplier,
                        source));
                    break;
                case ItemType.Tower:
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.Damage, DamageStrength * multiplier, source));
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.AttackSpeed, AttackSpeedStrength * multiplier,
                        source));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
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
                Stat.Damage => DamageStrength * 0.3f,
                Stat.AttackSpeed => AttackSpeedStrength * 0.3f,
                Stat.Armor => ArmorStrength * 0.3f,
                Stat.Health => HealthStrength * 0.3f,
                Stat.CritChance => Random.Range(0.2f, 0.3f) +
                                   Mathf.Lerp(0.2f, 0.5f, Mathf.Log(1 /*GameManager.DungeonFloor*/, 50f)),
                Stat.Dodge => Random.Range(0f, 0.1f) +
                              Mathf.Lerp(0.2f, 0.4f, Mathf.Log(1 /*GameManager.DungeonFloor*/, 50f)),
                Stat.AoeSize => Random.Range(0.2f, 0.75f),
                Stat.MoveSpeed => Random.Range(0f, 2f) +
                                  Mathf.Lerp(1f, 3f, Mathf.Log(1 /*GameManager.DungeonFloor*/, 50f)),
                Stat.CooldownReduce => Random.Range(0f, 0.1f) +
                                       Mathf.Lerp(0.2f, 0.4f, Mathf.Log(1 /*GameManager.DungeonFloor*/, 50f)),
                Stat.HpRegen => HealthStrength / 10f,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }

        #endregion

        public static float DungeonInteractMultiplier => (1 + GameManager.DungeonFloor / 5f);

        public static int GenerateCost(ItemRarity itemRarity, int dungeonFloor)
        {
            return (int) (itemRarity switch
            {
                ItemRarity.Common => 70f * (1.2f + dungeonFloor / 7f) + 80f,
                ItemRarity.Uncommon => 90f * (1.2f + dungeonFloor / 7f) + 80f,
                ItemRarity.Rare => 110f * (1.2f + dungeonFloor / 7f) + 80f,
                ItemRarity.Unique => 130f * (1.2f + dungeonFloor / 7f) + 80f,
                _ => 0
            });
        }
    }
}