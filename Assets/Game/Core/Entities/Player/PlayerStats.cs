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
    public enum Stat
    {
        AttackSpeed,
        Damage,
        CritChance,
        AoeSize, //Weapon stats, 0-3
        MoveSpeed,
        Health,
        Armor,
        Dodge,
        HpRegen //Armor Stats 4-8
    }

    public enum StatType
    {
        Flat,
        AdditivePercent,
        Multiplicative,
        Override
    }

    [Serializable]
    public struct StatBonus //Holds stat type, strength, and source.
    {
        public StatType statType;
        public Stat stat;
        public float strength;
        public string source;

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
        static PlayerStats()
        {
            OnStatsChanged += UpdateStatValues;
        }

        private static List<StatBonus> _activeStatBonuses = new(); //Holds all the active stat bonuses

        #region Stat Properties

        private static readonly Dictionary<Stat, StatHandler> StatValues = new();

        public delegate float StatHandler();

        //Dodge
        public static bool DodgeSuccessful => Random.Range(0f, 1f) < GetStat(Stat.Dodge);

        //Crit chance
        public static bool CritSuccessful => Random.Range(0f, 1f) < GetStat(Stat.CritChance);

        //Damage
        public static float OutgoingDamage => GetStat(Stat.Damage);

        public static float OutgoingDamageWithCrit => (CritSuccessful ? 2 : 1) * OutgoingDamage;

        //Armor
        public static float ModifyIncomingDamage(float damage)
        {
            return
                damage * (100f /
                          (100 + GetStat(Stat.Armor))); //100 armor = 50% reduction, 200 armor = 66% reduction, etc.
        }

        #endregion

        public delegate void StatAction();

        public static event StatAction OnStatsChanged;

        #region Stat List Accessing Methods

        public static float GetStat(Stat stat)
        {
            if (!StatValues.ContainsKey(stat)) return 0;
            return StatValues[stat].Invoke();
        }

        private static void UpdateStatValues()
        {
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                StatValues[stat] = () => GetStatValue(stat);
            }
        }

        private static float GetStatValue(Stat stat)
        {
            //Check if there is an override for the given stat
            var overrideBonus = _activeStatBonuses.Where(s => s.statType == StatType.Override && s.stat == stat)
                .ToArray();
            if (overrideBonus.Any()) return overrideBonus.FirstOrDefault().strength;
            //

            var bonusDictionary = MergeStatBonuses(_activeStatBonuses.Where(s => s.stat == stat));

            return bonusDictionary.ContainsKey(stat) ? bonusDictionary[stat] : 0;
        }

        private static Dictionary<Stat, float> MergeStatBonuses(IEnumerable<StatBonus> statBonuses)
        {
            Dictionary<Stat, float> bonusDictionary = new();

            var statBonusEnumerable = statBonuses.ToList();
            foreach (var stat in statBonusEnumerable.Select(s => s.stat).Distinct())
            {
                var flatBonuses = statBonusEnumerable.Where(s => s.statType == StatType.Flat && s.stat == stat)
                    .ToList();
                var flatBonus = flatBonuses.Any() ? flatBonuses.Sum(s => s.strength) : 0;
                var additivePercentBonuses = statBonusEnumerable
                    .Where(s => s.statType == StatType.AdditivePercent && s.stat == stat).ToList();
                var additivePercentBonus =
                    additivePercentBonuses.Any() ? additivePercentBonuses.Sum(s => s.strength) : 0;
                var multiplyBonuses = statBonusEnumerable
                    .Where(s => s.statType == StatType.Multiplicative && s.stat == stat).ToList();
                var multiply = multiplyBonuses.Any()
                    ? multiplyBonuses
                        .Select(s => s.strength)
                        .Aggregate(0f, (x, y) => x * y)
                    : 0;

                bonusDictionary[stat] = flatBonus * (1 + additivePercentBonus) * (1 + multiply);
            }

            return bonusDictionary;
        }

        public static void
            AddStatBonus(StatType statType, Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            _activeStatBonuses.Add(new StatBonus(statType, stat, strength, source));
            OnStatsChanged?.Invoke();
        }

        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.statBonuses)
            {
                _activeStatBonuses.Add(new StatBonus(sb.statType, sb.stat, sb.strength, sb.source));
                OnStatsChanged?.Invoke();
            }
        }

        public static void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
            OnStatsChanged?.Invoke();
        }

        public static void ClearStatBonuses()
        {
            _activeStatBonuses.Clear();
            OnStatsChanged?.Invoke();
        }

        private static IEnumerable<StatBonus>
            GetStatBonuses(Stat stat,
                bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            var foundBonuses = _activeStatBonuses.Where(sb => sb.stat == stat).ToArray();
            if (foundBonuses.Length != 0) return foundBonuses;
            return returnEmptyBonus ? new[] {new StatBonus(StatType.Flat, stat, 0, "Empty Bonus")} : foundBonuses;
        }

        #endregion

        #region Tool Methods

        public static Stat
            GetValidStatForItem(
                ItemType type) //Returns a random stat, based on valid stat types for different item types
        {
            return type switch
            {
                ItemType.Weapon => (Stat) Random.Range(0, 3),
                ItemType.Armor => (Stat) Random.Range(4, 8),
                _ => Stat.AttackSpeed //Returns attack speed by default.
            };
        }

        public static string
            GetStatsText(Stat[] stats,
                bool includeLabels,
                bool separateTypes) //Returns a text description of stats, based on the stats specified.
        {
            stats = stats.Distinct().ToArray();
            var statBonuses = new List<StatBonus>();
            foreach (var t in stats)
            {
                var statBonusArray = GetStatBonuses(t, true);
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
                var mergedStatBonuses = MergeStatBonuses(statBonusList);

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
            {Stat.MoveSpeed, "F1,/s"}
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