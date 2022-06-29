using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TonyDev.Game.Core.Items;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{
    public enum Stat
    {
        AttackSpeed, Damage, CritChance, CritDamage, AoeSize, Knockback, Stun, //Weapon stats, 0-6
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
        private static List<StatBonus> _activeStatBonuses = new (); //Holds all the active stat bonuses

        #region Stat Properties
        public delegate float StatHandler();
        //HP Regen
        public static StatHandler HpRegenHandler = () => 1f + GetStatBonus(Stat.HpRegen);
        public static float HpRegen => HpRegenHandler.Invoke();
        //Dodge
        public static StatHandler DodgeHandler = () => GetStatBonus(Stat.Dodge);
        public static bool DodgeSuccessful => Random.Range(0f, 1f) < DodgeHandler.Invoke();
        //Health
        public static StatHandler HealthHandler = () => 100 + (int)(GetStatBonus(Stat.Health) * 100);
        public static float Health => HealthHandler.Invoke();
        //Attack Speed
        public static StatHandler AttackSpeedHandler = () => 1 + GetStatBonus(Stat.AttackSpeed);
        public static float AttackSpeedMultiplier => AttackSpeedHandler.Invoke();
        //Crit chance
        public static StatHandler CritChanceHandler = () => GetStatBonus(Stat.CritChance);
        public static float CritChance => CritChanceHandler.Invoke();
        public static bool CritSuccessful => Random.Range(0f, 1f) < CritChance;
        //Crit damage
        public static StatHandler CritDamageHandler = () => 2f + GetStatBonus(Stat.CritDamage);
        public static float CritDamageMultiplier => CritDamageHandler.Invoke();
        //Damage
        public static StatHandler DamageHandler = () => 1f + GetStatBonus(Stat.Damage);
        public static float OutgoingDamageMultiplier => DamageHandler.Invoke();
        public static float OutgoingDamageMultiplierWithCrit => (CritSuccessful ? CritDamageMultiplier : 1) * OutgoingDamageMultiplier;
        //Knockback
        public static StatHandler KnockbackHandler = () => 1 + GetStatBonus(Stat.Knockback);
        public static float KnockbackMultiplier => KnockbackHandler.Invoke();
        //AoE
        public static StatHandler AoEHandler = () => 1 + GetStatBonus(Stat.AoeSize);
        public static float AoEMultiplier => AoEHandler.Invoke();
        //Move Speed
        public static StatHandler MoveSpeedHandler = () => 1 + GetStatBonus(Stat.MoveSpeed);
        public static float MoveSpeedMultiplier => MoveSpeedHandler.Invoke();
        //Armor
        public static StatHandler ArmorHandler = () => GetStatBonus(Stat.Armor);
        public static float IncomingDamageFlatReduction => ArmorHandler.Invoke();
        //Damage reduction
        public static StatHandler DamageReductionHandler = () => GetStatBonus(Stat.DamageReduction);
        public static float IncomingDamageMultiplier = 1f - DamageReductionHandler.Invoke();
        public static float ModifyIncomingDamage(float damage)
        {
            return (damage - IncomingDamageFlatReduction) * IncomingDamageMultiplier;
        }
        #endregion
        
        #region Stat List Accessing Methods
        public static void AddStatBonus(Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            _activeStatBonuses.Add(new StatBonus(stat, strength, source));
        }
    
        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.statBonuses)
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
        #endregion

        #region Tool Methods
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
                var value = t.strength;
                sb.Append(GetStatText(t.stat, value) + "\n");
            }

            return sb.ToString();
        }

        private static string GetStatText(Stat stat, float strength)
        {
            float.TryParse(strength.ToString("F2"), out var value);
            
            return stat switch
            {
                Stat.Armor => "Armor: " + value * 10f,
                Stat.Damage => ("Damage: " + ((int) (value * 100f)) + "%"),
                Stat.Dodge => ("Dodge: " + (int) (value * 100f) + "%"),
                Stat.Health => ("Max HP: " + ((int) (value * 100f))),
                Stat.Knockback => ("Knockback: " + ((int) (value * 100f)) + "%"),
                Stat.Stun => ("Stun: " + (int) (value * 100f) + "%"),
                Stat.Tenacity => ("Tenacity: " + (int) (value * 100f) + "%"),
                Stat.AoeSize => ("AoE Size: " + ((int) (value * 100f)) + "%"),
                Stat.AttackSpeed => "Atk Spd: " + ((int) (value * 100f)) + "%",
                Stat.CritChance => ("Crit Chance: " + (int) (value * 100f) + "%"),
                Stat.CritDamage => ("Crit Damage: " + ((int) (value * 100f)) + "%"),
                Stat.DamageReduction => ("Dmg Reduction: " + (int) (value * 100f) + "%"),
                Stat.HpRegen => ("Regen: " + (value) + "HP/sec"),
                Stat.MoveSpeed => ("Speed: " + ((int) (value * 100f)) + "%"),
                _ => string.Empty
            };
        }
        #endregion
    }
}