using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{

    [Serializable]
    public struct StatBonus //Holds stat type, strength, and source.
    {
        public StatType statType;
        public Stat stat;
        public float strength;
        [HideInInspector] public string source;

        public StatBonus(StatType statType, Stat stat, float strength, string source)
        {
            this.statType = statType;
            this.stat = stat;
            this.strength = strength;
            this.source = source;
        }

        public static List<StatBonus> Combine(IEnumerable<StatBonus> bonus1, IEnumerable<StatBonus> bonus2)
        {
            var statBonuses = bonus1.ToList();
            statBonuses.AddRange(bonus2);
            return statBonuses;
        }
    }

    public static class PlayerStats
    {
        public static EntityStats Stats = new ();

        #region Item Stats
        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.statBonuses)
            {
                Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, sb.source);
            }
        }
        
        public static Stat
            GetValidStatForItem(
                ItemType type) //Returns a random stat, based on valid stat types for different item types
        {
            return type switch
            {
                ItemType.Weapon => (Stat) Random.Range(0, 5),
                ItemType.Armor => (Stat) Random.Range(5, 10),
                _ => Stat.AttackSpeed //Returns attack speed by default.
            };
        }
        #endregion
        
        #region Stat Text
        public static string
            GetStatsText(Stat[] stats,
                bool includeLabels,
                bool separateTypes) //Returns a text description of stats, based on the stats specified.
        {
            stats = stats.Distinct().ToArray();
            var statBonuses = new List<StatBonus>();
            foreach (var t in stats)
            {
                var statBonusArray = Stats.GetStatBonuses(t, true);
                statBonuses.AddRange(statBonusArray);
            }

            return GetStatsTextFromBonuses(statBonuses, includeLabels, separateTypes);
        }

        public static string
            GetStatsTextFromBonuses(IEnumerable<StatBonus> statBonuses,
                bool includeLabels,
                bool separateTypes) //Returns a text description of stats, based on the stat bonuses specified.
        {
            var statBonusList = statBonuses.ToList();

            var stringBuilder = new StringBuilder();

            if (!separateTypes)
            {
                var mergedStatBonuses = EntityStats.MergeStatBonuses(statBonusList);

                foreach (var (key, value) in mergedStatBonuses)
                {
                    stringBuilder.AppendLine(StatToText(key, StatType.Override, value, includeLabels));
                }
            }
            else
            {
                foreach (var sb in statBonusList)
                {
                    stringBuilder.AppendLine(StatToText(sb.stat, sb.statType, sb.strength, includeLabels));
                }
            }

            return stringBuilder.ToString();
        }

        private static readonly Dictionary<Stat, string> StatFormattingKey = new Dictionary<Stat, string>()
        {
            {Stat.Armor, "F0,"},
            {Stat.Damage, "F0,"},
            {Stat.Dodge, "P0,"},
            {Stat.Health, "F0,"},
            {Stat.AoeSize, "P0,"},
            {Stat.AttackSpeed, "P0,"},
            {Stat.CritChance, "P0,"},
            {Stat.HpRegen, "F1,/s"},
            {Stat.MoveSpeed, "F1,/s"},
            {Stat.CooldownReduce, "P0,"}
        };

        private static string StatToText(Stat stat, StatType type, float strength, bool includeLabels)
        {
            var formatting = StatFormattingKey[stat].Split(',');
            
            return type switch
            {
                StatType.Override or StatType.Flat => (includeLabels ? Enum.GetName(typeof(Stat), stat) + ": " : "") +
                                                                    Tools.RemoveWhitespace(
                                                                        strength.ToString(formatting[0])) + formatting[1],
                StatType.AdditivePercent => (includeLabels ? Enum.GetName(typeof(Stat), stat) + ": " : "") + "+" +
                                                   Tools.RemoveWhitespace(strength.ToString("P0")) + formatting[1],
                StatType.Multiplicative => (includeLabels ? Enum.GetName(typeof(Stat), stat) + ": " : "") + "x" + 
                                                  Tools.RemoveWhitespace(strength.ToString("F2")) + formatting[1],
                _ => ""
            };
        }
        #endregion
    }
}