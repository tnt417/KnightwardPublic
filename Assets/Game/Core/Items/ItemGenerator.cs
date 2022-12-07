using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//Provides functionality for randomly generating items
namespace TonyDev.Game.Core.Items
{
    public static class ItemGenerator
    {
        private static Sprite[] _armorSprites; //Possible armor sprites to pick from

        public static void
            InitSprites() //Load sprites from SpriteDictionary based on the prefix to be used in randomly generated items.
        {
            _armorSprites = ObjectFinder.GetSpritesWithPrefix("ai_");
        }

        private static Item GetTowerItem(ItemRarity itemRarity)
        {
            return Tools.SelectRandom(GameManager.AllItems.Select(id => Object.Instantiate(id).item).Where(i => i.itemType == ItemType.Tower && i.itemRarity == itemRarity).ToArray());
        }
        
        public static Item GenerateItem(int rarityBoost)
        {
            var itemType = Item.RandomItemType;

            return GenerateItemOfType(itemType, Item.RandomRarity(rarityBoost));
        }

        public static Item GenerateItemFromData(ItemData data)
        {
            var item = Object.Instantiate(data).item;

            item.statBonuses = StatBonus.Combine(GenerateItemStats(item.itemType, item.itemRarity, item.bypassStatGeneration), item.statBonuses).ToArray();

            if (item.itemType == ItemType.Relic)
            {
                return ScaleRelicStats(item);
            }
            
            return item;
        }
        
        public static int GenerateEssence(Item item, int dungeonFloor = 0)
        {
            if (dungeonFloor <= 0) dungeonFloor = GameManager.DungeonFloor;
            
            if (item == null)
            {
                Debug.LogWarning("Cannot generate essence for null item!");
                return 0;
            }

            return (int) Mathf.Pow(GenerateCost(item, dungeonFloor) * Random.Range(0.4f, 0.5f), 1.3f);
        }

        //Returns a randomly generated item based on input parameters
        public static Item GenerateItemOfType(ItemType type, ItemRarity rarity)
        {

            //Instantiate local variables that will end up at the parameters for the generated item.
            Item item = null;
            var itemName = string.Empty;
            Sprite sprite = null;
            var bypassStatGen = false;
            //

            if (type == ItemType.Weapon)
            {
                item = Tools.SelectRandom(GameManager.AllItems.Select(id => Object.Instantiate(id).item).Where(i => i.itemType == ItemType.Weapon));
            }

            //Set sprites and item names based on the item type
            switch (type)
            {
                case ItemType.Weapon:
                    if (item == null)
                    {
                        Debug.LogWarning("Failed to generate weapon item!");
                        return null;
                    }
                    sprite = item.uiSprite;
                    itemName = item.itemName;
                    break;
                case ItemType.Armor:
                    sprite = Tools.SelectRandom(_armorSprites);
                    itemName = "Armor";
                    break;
                case ItemType.Relic:
                    var selectedRelic = Tools.SelectRandom(GameManager.AllItems.Select(id => Object.Instantiate(id).item).Where(i => i.itemType == ItemType.Relic && i.itemRarity == rarity));
                    bypassStatGen = selectedRelic.bypassStatGeneration;
                    return ScaleRelicStats(selectedRelic);
                case ItemType.Tower:
                    item = GetTowerItem(rarity);
                    bypassStatGen = item.bypassStatGeneration;
                    item.statBonuses = GenerateItemStats(item.itemType, item.itemRarity, bypassStatGen);
                    return item;
            }

            //Return a new item using all the variables this function has generated and/or been provided with
            return new Item
            {
                itemType = type,
                itemRarity = rarity,
                itemName = itemName,
                statBonuses = item == null ? GenerateItemStats(type, rarity, bypassStatGen) : StatBonus.Combine(GenerateItemStats(type, rarity, bypassStatGen), item.statBonuses).ToArray(),
                uiSprite = sprite,
                itemEffects = item?.itemEffects.AsReadOnly().ToList(),
                projectiles = item?.projectiles
            };
        }

        #region Stat Generation

        public static float StatStrengthFactor => 1f + GameManager.DungeonFloor/15f;
        private static float DamageStrength => Random.Range(0.95f, 1.05f) * GameManager.DungeonFloor * 1.75f + 15f;
        private static float AttackSpeedStrength => Random.Range(0.95f, 1.05f) + Mathf.Log(GameManager.DungeonFloor, 50);
        private static float ArmorStrength => Random.Range(0.95f, 1.05f) * 5 * GameManager.DungeonFloor;
        private static float HealthStrength => Random.Range(0.95f, 1.05f) * 20 * GameManager.DungeonFloor;

        private static Item ScaleRelicStats(Item original)
        {
            for (var i = 0; i < original.statBonuses.Length; i++)
            {
                var bonus = original.statBonuses[i];

                bonus.strength *= Mathf.Pow(0.8f + Mathf.Log(StatStrengthFactor), 2);

                original.statBonuses[i] = bonus;
            }
            
            return original;
        }
        
        private static StatBonus[] GenerateItemStats(ItemType itemType, ItemRarity itemRarity, bool bypassStatGen)
        {
            if (itemType is not (ItemType.Armor or ItemType.Weapon or ItemType.Tower) || bypassStatGen) return null;

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
                case ItemType.Tower:
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.Damage, DamageStrength * multiplier, source));
                    statBonuses.Add(new StatBonus(StatType.Flat, Stat.AttackSpeed, AttackSpeedStrength * multiplier, source));
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
                Stat.Damage => DamageStrength*0.3f,
                Stat.AttackSpeed => AttackSpeedStrength*0.3f,
                Stat.Armor => ArmorStrength * 0.3f,
                Stat.Health => HealthStrength * 0.3f,
                Stat.CritChance => Random.Range(0.2f, 0.3f) + Mathf.Lerp(0.2f, 0.5f, Mathf.Log(GameManager.DungeonFloor, 50f)),
                Stat.Dodge => Random.Range(0f, 0.1f) + Mathf.Lerp(0.2f, 0.4f, Mathf.Log(GameManager.DungeonFloor, 50f)),
                Stat.AoeSize => Random.Range(0.2f, 0.75f),
                Stat.MoveSpeed => Random.Range(0f, 2f) + Mathf.Lerp(1f, 3f, Mathf.Log(GameManager.DungeonFloor, 50f)),
                Stat.CooldownReduce => Random.Range(0f, 0.1f) + Mathf.Lerp(0.2f, 0.4f, Mathf.Log(GameManager.DungeonFloor, 50f)),
                Stat.HpRegen => HealthStrength / 10f,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
        #endregion

        public static float DungeonInteractMultiplier => (1 + GameManager.DungeonFloor / 5f);
        
        public static int GenerateCost(Item item, int dungeonFloor)
        {
            if (item == null) return 0;
            
            return (int) (item.itemRarity switch
            {
                ItemRarity.Common => 10f * (1 + dungeonFloor/5f),
                ItemRarity.Uncommon => 15f * (1 + dungeonFloor/5f),
                ItemRarity.Rare => 20f * (1 + dungeonFloor/5f),
                ItemRarity.Unique => 25f * (1 + dungeonFloor/5f),
                _ => 0
            });
        }
    }
}