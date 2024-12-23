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
        public bool hidden;
        
        public StatBonus(StatType statType, Stat stat, float strength, string source)
        {
            this.statType = statType;
            this.stat = stat;
            this.strength = strength;
            this.source = source;
            this.hidden = false;
        }
        
        public StatBonus(StatType statType, Stat stat, float strength, string source, bool hidden = false)
        {
            this.statType = statType;
            this.stat = stat;
            this.strength = strength;
            this.source = source;
            this.hidden = hidden;
        }

        public static List<StatBonus> Combine(IEnumerable<StatBonus> bonus1, IEnumerable<StatBonus> bonus2)
        {
            if (bonus1 == null) return bonus2.ToList();
            if (bonus2 == null) return bonus1.ToList();

            var statBonuses = bonus1.ToList();
            statBonuses.AddRange(bonus2);
            return statBonuses;
        }
    }

    public static class PlayerStats
    {
        public static EntityStats Stats => Player.LocalInstance == null ? null : Player.LocalInstance.Stats;

        #region Item Stats

        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.statBonuses)
            {
                Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, item.itemName);
            }
        }

        public static Stat
            GetValidStatForItem(
                ItemType type) //Returns a random stat, based on valid stat types for different item types
        {
            return type switch
            {
                ItemType.Weapon => (Stat) Random.Range(0, 5),
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
                var statBonusArray = Stats.GetStatBonuses(t, true).Where(sb => !sb.hidden);
                statBonuses.AddRange(statBonusArray);
            }

            return GetStatsTextFromBonuses(statBonuses, includeLabels, separateTypes);
        }

        public static string
            GetStatsTextFromBonuses(IEnumerable<StatBonus> statBonuses,
                bool includeLabels,
                bool separateTypes,
                bool tower = false) //Returns a text description of stats, based on the stat bonuses specified.
        {
            if (statBonuses == null) return "";

            statBonuses = statBonuses.Where(sb => !sb.hidden);
            
            var statBonusList = statBonuses.ToList();

            var stringBuilder = new StringBuilder();

            if (!separateTypes)
            {
                var mergedStatBonuses = EntityStats.MergeStatBonuses(statBonusList);

                foreach (var (key, value) in mergedStatBonuses)
                {
                    stringBuilder.AppendLine(StatToText(key, StatType.Override, value, includeLabels, tower));
                }
            }
            else
            {
                foreach (var stat in Enum.GetValues(typeof(Stat)).Cast<Stat>()) //Loop every stat
                {
                    if (statBonusList.All(sb => sb.stat != stat)) continue; //Skip if no stats of stat type

                    if (statBonusList.Any(sb =>
                        sb.stat == stat && sb.statType == StatType.Flat)) //If there are flat stat types...
                    {
                        var flatStrength = statBonusList.Where(sb => sb.stat == stat && sb.statType == StatType.Flat)
                            .Select(sb => sb.strength)
                            .Sum(); //...Sum them up...

                        stringBuilder.AppendLine(StatToText(stat, StatType.Flat, flatStrength, includeLabels,
                            tower)); //...And append the text.
                    }

                    if (statBonusList.Any(sb =>
                        sb.stat == stat &&
                        sb.statType == StatType.AdditivePercent)) //If there are additive stat types...
                    {
                        var additiveStrength = statBonusList
                            .Where(sb => sb.stat == stat && sb.statType == StatType.AdditivePercent)
                            .Select(sb => sb.strength)
                            .Sum(); //...Sum them up...

                        stringBuilder.AppendLine(StatToText(stat, StatType.AdditivePercent, additiveStrength,
                            includeLabels, tower)); //...And append the text.
                    }

                    if (statBonusList.Any(sb =>
                        sb.stat == stat &&
                        sb.statType == StatType.Multiplicative)) //If there are multiplicative stat types...
                    {
                        var multStrength = statBonusList
                            .Where(sb => sb.stat == stat && sb.statType == StatType.Multiplicative)
                            .Select(sb => sb.strength).Aggregate((total, next) => total * next); //...Product them...

                        stringBuilder.AppendLine(StatToText(stat, StatType.Multiplicative, multStrength, includeLabels, tower)); //...And append the text.
                    }
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
            {Stat.AttackSpeed, "F1,"},
            {Stat.CritChance, "P0,"},
            {Stat.HpRegen, "F1,/s"},
            {Stat.MoveSpeed, "F1,/s"},
            {Stat.CooldownReduce, "P0,"}
        };

        public static readonly Dictionary<Stat, string> StatLabelKey = new Dictionary<Stat, string>()
        {
            {Stat.Armor, "Armor"},
            {Stat.Damage, "Damage"},
            {Stat.Dodge, "Dodge Chance"},
            {Stat.Health, "Health"},
            {Stat.AoeSize, "Area of Effect"},
            {Stat.AttackSpeed, "Attack Speed"},
            {Stat.CritChance, "Critical Chance"},
            {Stat.HpRegen, "Health Regen"},
            {Stat.MoveSpeed, "Move Speed"},
            {Stat.CooldownReduce, "Cooldown Reduction"}
        };

        private static readonly Dictionary<Stat, string> TowerStatLabelKey = new Dictionary<Stat, string>()
        {
            {Stat.Armor, "Armor"},
            {Stat.Damage, "Strength"},
            {Stat.Dodge, "Dodge Chance"},
            {Stat.Health, "Durability"},
            {Stat.AoeSize, "Area of Effect"},
            {Stat.AttackSpeed, "Frequency"},
            {Stat.CritChance, "Critical Chance"},
            {Stat.HpRegen, "Regen"},
            {Stat.MoveSpeed, "Speed"},
            {Stat.CooldownReduce, "Cooldown Reduction"}
        };
        
        private static readonly Dictionary<Stat, int> SpriteLabelKey = new Dictionary<Stat, int>()
        {
            {Stat.Armor, 7},
            {Stat.Damage, 3},
            {Stat.Dodge, 10},
            {Stat.Health, 4},
            {Stat.AoeSize, 6},
            {Stat.AttackSpeed, 11},
            {Stat.CritChance, 9},
            {Stat.HpRegen, 5},
            {Stat.MoveSpeed, 8},
            {Stat.CooldownReduce, 12}
        };

        private static string StatToText(Stat stat, StatType type, float strength, bool includeLabels,
            bool isTower)
        {
            var sb = new StringBuilder();
            var formatting = StatFormattingKey[stat].Split(',');

            var colorTag = type switch
            {
                StatType.Flat => strength > 0 ? "#63ab3f" : "#e64539",
                StatType.Multiplicative => strength > 1 ? "#63ab3f" : "#e64539",
                StatType.AdditivePercent => strength > 0 ? "#63ab3f" : "#e64539",
                _ => "white"
            };

            sb.Append("<color=" + colorTag + ">");

            if(includeLabels) sb.Append("<sprite=" + SpriteLabelKey[stat] + ">");
            
            switch (type)
            {
                case StatType.Override or StatType.Flat:
                    sb.Append($"{GameTools.RemoveWhitespace(strength.ToString(formatting[0])) + formatting[1],-3}");
                    break;
                case StatType.AdditivePercent:
                    sb.Append(
                        $"{(strength > 0 ? "+" : "") + GameTools.RemoveWhitespace(strength.ToString("P0")) + " total",-3}");
                    break;
                case StatType.Multiplicative:
                    sb.Append($"{"x" + GameTools.RemoveWhitespace(strength.ToString("F1")),-3}");
                    break;
                default:
                    break;
            }

            sb.Append("</color>");
            
            sb.Append(" " + (includeLabels ? (isTower ? TowerStatLabelKey[stat] : StatLabelKey[stat]) : ""));

            return sb.ToString();
        }

        #endregion
    }
}