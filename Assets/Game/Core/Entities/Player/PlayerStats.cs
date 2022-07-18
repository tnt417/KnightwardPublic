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
        AttackSpeed, Damage, CritChance, AoeSize, //Weapon stats, 0-3
        MoveSpeed, Health, Armor, Dodge, HpRegen //Armor Stats 4-8
    }

    public enum StatType
    {
        Flat, Multiplicative, Override
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
    }

    public static class PlayerStats
    {
        private static List<StatBonus> _activeStatBonuses = new (); //Holds all the active stat bonuses

        #region Stat Properties
        public delegate float StatHandler();
        //HP Regen
        public static StatHandler HpRegenHandler = () => (1f + GetFlatStatBonus(Stat.HpRegen)) * GetStatMultiplyBonus(Stat.HpRegen);
        public static float HpRegen => HpRegenHandler.Invoke();
        //Dodge
        public static StatHandler DodgeHandler = () => GetFlatStatBonus(Stat.Dodge) * GetStatMultiplyBonus(Stat.Dodge);
        public static bool DodgeSuccessful => Random.Range(0f, 1f) < DodgeHandler.Invoke();
        //Health
        public static StatHandler HealthHandler = () => (100 + (int)GetFlatStatBonus(Stat.Health)) * GetStatMultiplyBonus(Stat.Health);
        public static float Health => HealthHandler.Invoke();
        //Attack Speed
        public static StatHandler AttackSpeedHandler = () => GetFlatStatBonus(Stat.AttackSpeed) * GetStatMultiplyBonus(Stat.AttackSpeed);
        public static float AttackSpeedMultiplier => AttackSpeedHandler.Invoke();
        //Crit chance
        public static StatHandler CritChanceHandler = () => GetFlatStatBonus(Stat.CritChance) * GetStatMultiplyBonus(Stat.CritChance);
        public static float CritChance => CritChanceHandler.Invoke();
        public static bool CritSuccessful => Random.Range(0f, 1f) < CritChance;
        /*//Crit damage
        public static StatHandler CritDamageHandler = () => 2f + GetFlatStatBonus(Stat.CritDamage) * GetStatMultiplyBonus(Stat.CritDamage);
        public static float CritDamageMultiplier => CritDamageHandler.Invoke();*/
        //Damage
        public static StatHandler DamageHandler = () => GetFlatStatBonus(Stat.Damage) * GetStatMultiplyBonus(Stat.Damage);
        public static float OutgoingDamage => DamageHandler.Invoke();
        public static float OutgoingDamageWithCrit => (CritSuccessful ? 2 : 1) * OutgoingDamage;
        /*//Knockback
        public static StatHandler KnockbackHandler = () => (1 + GetFlatStatBonus(Stat.Knockback)) * GetStatMultiplyBonus(Stat.Knockback);
        public static float KnockbackMultiplier => KnockbackHandler.Invoke();*/
        //AoE
        public static StatHandler AoEHandler = () => (1 + GetFlatStatBonus(Stat.AoeSize)) * GetStatMultiplyBonus(Stat.AoeSize);
        public static float AoEMultiplier => AoEHandler.Invoke();
        //Move Speed
        public static StatHandler MoveSpeedHandler = () => (1 + GetFlatStatBonus(Stat.MoveSpeed)) * GetStatMultiplyBonus(Stat.MoveSpeed);
        public static float MoveSpeedMultiplier => MoveSpeedHandler.Invoke();
        //Armor
        public static StatHandler ArmorHandler = () => GetFlatStatBonus(Stat.Armor) * GetStatMultiplyBonus(Stat.Armor);
        //Damage reduction
        /*public static StatHandler DamageReductionHandler = () => GetFlatStatBonus(Stat.DamageReduction) * GetStatMultiplyBonus(Stat.DamageReduction);
        public static float IncomingDamageMultiplier = 1f - DamageReductionHandler.Invoke();*/
        //Tenacity
        /*public static StatHandler NegativeEffectMultiplierHandler = () => 1 - GetFlatStatBonus(Stat.Tenacity) * GetStatMultiplyBonus(Stat.Tenacity);
        public static float NegativeEffectMultiplier = NegativeEffectMultiplierHandler.Invoke();*/
        public static float ModifyIncomingDamage(float damage)
        {
            return damage * (100f/(100+ArmorHandler.Invoke())); //100 armor = 50% reduction, 200 armor = 66% reduction, etc.
        }
        #endregion
        
        #region Stat List Accessing Methods
        public static void AddStatBonus(StatType statType, Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            _activeStatBonuses.Add(new StatBonus(statType, stat, strength, source));
        }
    
        public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
        {
            foreach (var sb in item.statBonuses)
            {
                _activeStatBonuses.Add(new StatBonus(sb.statType, sb.stat, sb.strength, sb.source));
            }
        }

        public static void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
        }

        public static void ClearStatBonuses()
        {
            _activeStatBonuses.Clear();
        }

        public static float GetStatMultiplyBonus(Stat stat)
        {
            return 1 + _activeStatBonuses.Where(sb => sb.statType == StatType.Multiplicative && sb.stat == stat)
                .Sum(sb => sb.strength);
        }
        
        public static float GetFlatStatBonus(Stat stat) //Returns a sum of all stat bonuses of a certain type.
        {
            return _activeStatBonuses.Where(sb => sb.statType == StatType.Flat && sb.stat == stat).Sum(sb => sb.strength);
        }

        private static IEnumerable<StatBonus> GetStatBonuses(Stat stat, bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            var foundBonuses = _activeStatBonuses.Where(sb => sb.stat == stat).ToArray();
            if (foundBonuses.Length != 0) return foundBonuses;
            return returnEmptyBonus ? new[] {new StatBonus(StatType.Flat, stat, 0, "Empty Bonus")} : foundBonuses;
        }
        #endregion

        #region Tool Methods
        public static Stat GetValidStatForItem(ItemType type) //Returns a random stat, based on valid stat types for different item types
        {
            return type switch
            {
                ItemType.Weapon => (Stat) Random.Range(0, 3),
                ItemType.Armor => (Stat) Random.Range(4, 8),
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
                sb.AppendLine(GetStatText(t.stat, value));
            }

            return sb.ToString();
        }

        private static string GetStatText(Stat stat, float strength)
        {
            float.TryParse(strength.ToString("F2"), out var value);
            
            return stat switch
            {
                Stat.Armor => "Armor: " + value,
                Stat.Damage => "Damage: " + (int) value,
                Stat.Dodge => ("Dodge: " + (int) (value * 100f) + "%"),
                Stat.Health => ("Max HP: " + ((int) (value))),
                //Stat.Knockback => ("Knockback: " + ((int) (value * 100f)) + "%"),
                //Stat.Stun => ("Stun: " + (int) (value * 100f) + "%"),
                //Stat.Tenacity => ("Tenacity: " + (int) (value * 100f) + "%"),
                //Stat.AoeSize => ("AoE Size: " + ((int) (value * 100f)) + "%"),
                Stat.AttackSpeed => "Atk Spd: " + ((int) (value * 100f)) + "%",
                Stat.CritChance => ("Crit Chance: " + (int) (value * 100f) + "%"),
                //Stat.CritDamage => ("Crit Damage: " + ((int) (value * 100f)) + "%"),
                //Stat.DamageReduction => ("Dmg Reduction: " + (int) (value * 100f) + "%"),
                //Stat.HpRegen => ("Regen: " + (value) + "HP/sec"),
                Stat.MoveSpeed => ("Speed: " + ((int) (value * 100f)) + "%"),
                _ => string.Empty
            };
        }
        #endregion
    }
}