using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Items
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Relic,
        Tower
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Unique
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
                    >= 75 => ItemType.Armor,
                    >= 50 => ItemType.Relic,
                    >= 25 => ItemType.Weapon,
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
        [TextArea(3, 3)] public string itemDescription;
        public ItemType itemType;
        public ItemRarity itemRarity;
        public StatBonus[] statBonuses;
        [SerializeReference] [SerializeField] public List<GameEffect> itemEffects;

        public ProjectileData[] projectiles;
        //

        public GameObject spawnablePrefab => ObjectFinder.GetPrefab(spawnablePrefabName);
        public string spawnablePrefabName;

        public void AddInflictEffect(GameEffect effect, bool allProjectiles)
        {
            if (allProjectiles)
            {
                foreach (var t in projectiles)
                {
                    t.effects.Add(effect);
                }
            }
            else
            {
                projectiles[0].effects.Add(effect);
            }
        }

        public void RemoveInflictEffect(GameEffect effect)
        {
            foreach (var t in projectiles)
            {
                if (t.effects.Contains(effect)) t.effects.Remove(effect);
            }
        }

        public static ItemRarity RandomRarity(int rarityBoost)
        {
            //Roll for a random rarity
            var rarityRoll = Random.Range(0, 100) + rarityBoost;
            var itemRarity = rarityRoll switch
            {
                >= 95 => ItemRarity.Unique, //5% chance for unique
                >= 75 => ItemRarity.Rare, //20% chance for rare
                >= 40 => ItemRarity.Uncommon, //35% chance for uncommon
                >= 0 => ItemRarity.Common, //40% chance for common
                _ => ItemRarity.Common
            };
            //

            return itemRarity;
        }

        public string
            GetItemDescription() //Returns a string that contains a specified item's name and stats, all on their own line
        {
            var stringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(itemDescription)) stringBuilder.AppendLine(itemDescription);

            if (IsEquippable)
            {
                if (itemEffects is {Count: > 0})
                {
                    foreach (var ge in itemEffects.Where(ge => ge != null))
                    {
                        var desc = ge.GetEffectDescription();

                        if (string.IsNullOrEmpty(desc)) continue;

                        stringBuilder.AppendLine(desc);
                    }
                }

                stringBuilder.Append(PlayerStats.GetStatsTextFromBonuses(statBonuses, true, true, true));
            }

            return stringBuilder.ToString(); //Return the string
        }
    }
}