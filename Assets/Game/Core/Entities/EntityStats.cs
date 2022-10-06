using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities
{
    public enum Stat
    {
        AttackSpeed,
        Damage,
        CritChance,
        AoeSize,
        CooldownReduce, //Weapon stats, 0-4
        MoveSpeed,
        Health,
        Armor,
        Dodge,
        HpRegen //Armor Stats 5-9
    }

    public enum StatType
    {
        Flat,
        AdditivePercent,
        Multiplicative,
        Override
    }

    public class EntityStats
    {
        public EntityStats()
        {
            OnStatsChanged += UpdateStatValues;
        }

        private List<StatBonus> _activeStatBonuses = new();

        public delegate void StatAction();

        public event StatAction OnStatsChanged;

        private delegate float StatHandler();

        #region Stat Properties

        //Dodge
        public bool DodgeSuccessful => Random.Range(0f, 1f) < GetStat(Stat.Dodge);

        //Crit chance
        public bool CritSuccessful => Random.Range(0f, 1f) < GetStat(Stat.CritChance);

        //Damage
        public float OutgoingDamage => GetStat(Stat.Damage);

        //Armor
        public float ModifyIncomingDamage(float damage)
        {
            return
                damage * (100f /
                          (100 + GetStat(Stat.Armor))); //100 armor = 50% reduction, 200 armor = 66% reduction, etc.
        }

        #endregion

        #region Stat List Accessing Methods

        public Dictionary<Stat, float> StatValues = new();

        public bool ReadOnly = true; //Should this class only be responsible for returning values? Used to allow stat networking where only one client has control.
        
        public void ReplaceStatValueDictionary(Stat[] keys, float[] values)
        {
            var newDictionary = new Dictionary<Stat, float>();
            for (var i = 0; i < keys.Length; i++)
            {
                newDictionary.Add(keys[i], values[i]);
            }
            StatValues = newDictionary;
        }
        
        public float GetStat(Stat stat)
        {
            if (_buffTimers.Count > 0 && !ReadOnly)
                RemoveBuffs(_buffTimers.Where(kv => kv.Value < Time.time).Select(kv => kv.Key)); //Remove expired buffs

            if (!StatValues.ContainsKey(stat)) return 0;
            return StatValues[stat];
        }

        private void UpdateStatValues()
        {
            if (ReadOnly) return;
            
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                StatValues[stat] = GetStatValue(stat);
            }
        }

        private float GetStatValue(Stat stat)
        {
            if (ReadOnly) return 0;
            
            //Check if there is an override for the given stat
            var overrideBonus = _activeStatBonuses.Where(s => s.statType == StatType.Override && s.stat == stat)
                .ToArray();
            if (overrideBonus.Any()) return overrideBonus.FirstOrDefault().strength;
            //

            var bonusDictionary = MergeStatBonuses(_activeStatBonuses.Where(s => s.stat == stat));

            return bonusDictionary.ContainsKey(stat) ? bonusDictionary[stat] : 0;
        }

        //Combines similar stats while doing calculations between the stat types. Returns a dictionary with combined value of each unique stat.
        public static Dictionary<Stat, float> MergeStatBonuses(IEnumerable<StatBonus> statBonuses)
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
                        .Aggregate((x, y) => x * y)
                    : 1;

                bonusDictionary[stat] = flatBonus * (1 + additivePercentBonus) * multiply;
            }

            return bonusDictionary;
        }

        public void
            AddStatBonus(StatType statType, Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            if (ReadOnly) return;
            
            _activeStatBonuses.Add(new StatBonus(statType, stat, strength, source));
            OnStatsChanged?.Invoke();
        }

        public void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            if (ReadOnly) return;
            
            _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
            OnStatsChanged?.Invoke();
        }

        public void ClearStatBonuses()
        {
            if (ReadOnly) return;
            
            _activeStatBonuses.Clear();
            OnStatsChanged?.Invoke();
        }

        public IEnumerable<StatBonus>
            GetStatBonuses(Stat stat,
                bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            if (ReadOnly) return null;
            
            var foundBonuses = _activeStatBonuses.Where(sb => sb.stat == stat).ToArray();
            if (foundBonuses.Length != 0) return foundBonuses;
            return returnEmptyBonus ? new[] {new StatBonus(StatType.Flat, stat, 0, "Empty Bonus")} : foundBonuses;
        }

        #endregion

        #region Buffs

        private readonly Dictionary<string, float>
            _buffTimers = new(); //string is the source of the buff, float is the Time.time that the buff will expire

        private int _buffIndex = 0; //Used to ensure unique buff source IDs

        public void AddBuff(StatBonus bonus, float time)
        {
            if (ReadOnly) return;
            
            var source = bonus.source + "_BUFF" + _buffIndex;
            _buffIndex++;

            AddStatBonus(bonus.statType, bonus.stat, bonus.strength, source);
            _buffTimers.Add(source, Time.time + time);
            OnStatsChanged?.Invoke();
        }

        public void ClearBuffs()
        {
            if (ReadOnly) return;
            
            _activeStatBonuses.RemoveAll(sb => sb.source.Contains("_BUFF"));
            OnStatsChanged?.Invoke();
        }

        public void RemoveBuffs(IEnumerable<string> buffIDs)
        {
            if (ReadOnly) return;
            
            foreach (var bid in buffIDs.ToArray())
            {
                if (!_buffTimers.ContainsKey(bid))
                {
                    continue;
                }

                _buffTimers.Remove(bid);
                RemoveStatBonuses(bid);
            }
        }
        
        public void RemoveBuffsOfSource(string source)
        {
            if (ReadOnly) return;
            
            var keys = _buffTimers.Where(kv => kv.Key.StartsWith(source + "_BUFF")).Select(kv => kv.Key);

            RemoveBuffs(keys);
        }

        #endregion
    }
}