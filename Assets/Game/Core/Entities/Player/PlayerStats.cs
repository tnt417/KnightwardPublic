using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TonyDev.Game.Core.Items;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{
    public enum Stat
    {
        AttackSpeed, Damage, CritChance, CritDamage, AoeSize, Knockback, Stun, //Weapon stats, 0-6 //All valid for relics right now. 0-13
        MoveSpeed, Health, Armor, Tenacity, HpRegen, Dodge, DamageReduction //Armor Stats 7-13
    }

    [Serializable]
    public struct StatBonus //Holds stat type, strength, and source.
    {
        public Stat stat;
        public float strength;
        public string source;
        public StatBonus(Stat stat, float strength, string source)
        {
            this.stat = stat;
            this.strength = strength;
            this.source = source;
        }
    }

    public static class PlayerStats
    {
        private static List<StatBonus> _activeStatBonuses = new List<StatBonus>(); //Holds all the active stat bonuses

        public static void AddStatBonus(Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            _activeStatBonuses.Add(new StatBonus(stat, strength, source));
        }
    
        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.StatBonuses)
            {
                _activeStatBonuses.Add(new StatBonus(sb.stat, sb.strength, sb.source));
            }
        }

        public static void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
        }

        public static float GetStatBonus(Stat stat) //Returns a sum of all stat bonuses of a certain type.
        {
            return _activeStatBonuses.Where(sb => sb.stat == stat).Sum(sb => sb.strength);
        }

        public static IEnumerable<StatBonus> GetStatBonuses(Stat stat, bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            var foundBonuses = _activeStatBonuses.Where(sb => sb.stat == stat).ToArray();
            if (foundBonuses.Length != 0) return foundBonuses;
            return returnEmptyBonus ? new[] {new StatBonus(stat, 0, "Empty Bonus")} : foundBonuses;
        }

        public static Stat GetValidStatForItem(ItemType type) //Returns a random stat, based on valid stat types for different item types
        {
            return type switch
            {
                ItemType.Weapon => (Stat) Random.Range(0, 6),
                ItemType.Armor => (Stat) Random.Range(7, 13),
                ItemType.Relic => (Stat) Random.Range(0, 13),
                _ => Stat.AttackSpeed //Returns attack speed by default.
            };
        }

        public static string GetStatsText(Stat[] stats) //Returns a text description of stats, based on the stats specified.
        {
            stats = stats.Distinct().ToArray();
            var statBonuses = new List<StatBonus>();
            foreach (var t in stats)
            {
                var statBonusArray = GetStatBonuses(t, true);
                statBonuses.AddRange(statBonusArray);
            }
            return GetStatsText(statBonuses.ToArray());
        }
        public static string GetStatsText(IEnumerable<StatBonus> statBonuses) //Returns a text description of stats, based on the stat bonuses specified.
        {
            var sb = new StringBuilder();
            foreach (var t in statBonuses)
            {
                var statBonus = t.strength;
                switch (t.stat)
                {
                    case Stat.Armor:
                        sb.AppendLine("Armor: " + statBonus * 10f);
                        break;
                    case Stat.Damage:
                        sb.AppendLine("Damage: " + (100 + (int)(statBonus * 100f)) + "%");
                        break;
                    case Stat.Dodge:
                        sb.AppendLine("Dodge: " + (int)(statBonus * 100f) + "%");
                        break;
                    case Stat.Health:
                        sb.AppendLine("Max HP: " + (100 + (int)(statBonus * 100f)));
                        break;
                    case Stat.Knockback:
                        sb.AppendLine("Knockback: " + (100 + (int)(statBonus * 100f)) + "%");
                        break;
                    case Stat.Stun:
                        sb.AppendLine("Stun: " + (int)(statBonus * 100f) + "%");
                        break;
                    case Stat.Tenacity:
                        sb.AppendLine("Tenacity: " + (int)(statBonus * 100f) + "%");
                        break;
                    case Stat.AoeSize:
                        sb.AppendLine("AoE Size: " + (100 + (int)(statBonus * 100f)) + "%");
                        break;
                    case Stat.AttackSpeed:
                        sb.AppendLine("Atk Spd: " + (100 + (int)(statBonus * 100f)) + "%");
                        break;
                    case Stat.CritChance:
                        sb.AppendLine("Crit Chance: " + (int)(statBonus * 100f) + "%");
                        break;
                    case Stat.CritDamage:
                        sb.AppendLine("Crit Damage: " + (200 + (int)(statBonus * 100f)) + "%");
                        break;
                    case Stat.DamageReduction:
                        sb.AppendLine("Dmg Reduction: " + (int)(statBonus * 100f) + "%");
                        break;
                    case Stat.HpRegen:
                        sb.AppendLine("Regen: " + ( 1 + statBonus) + "HP/sec");
                        break;
                    case Stat.MoveSpeed:
                        sb.AppendLine("Speed: " + (100 + (int)(statBonus * 100f)) + "%");
                        break;
                }
            }

            return sb.ToString();
        }
    }
}