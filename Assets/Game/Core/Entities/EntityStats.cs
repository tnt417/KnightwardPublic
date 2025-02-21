using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
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
            OnStatChanged += UpdateStatValue;
        }

        private readonly Dictionary<Stat, List<StatBonus>> _activeStatBonuses = new();

        public Action<Stat> OnStatChanged;

        #region Stat Properties

        //Dodge
        public bool DodgeSuccessful => Random.Range(0f, 1f) < Mathf.Clamp(GetStat(Stat.Dodge), 0, 0.75f);

        //Crit chance
        public bool CritSuccessful => Random.Range(0f, 1f) < GetStat(Stat.CritChance);

        //Damage
        public float OutgoingDamage => GetStat(Stat.Damage);

        //Armor
        public float ModifyIncomingDamage(float damage)
        {
            return
                damage * (100f /
                          (100 + Mathf.Pow(Mathf.Clamp(GetStat(Stat.Armor),0 ,Mathf.Infinity)*2, 0.8f))); //100 armor = 50% reduction, 200 armor = 66% reduction, etc.
        }

        #endregion

        #region Stat List Accessing Methods

        public Dictionary<Stat, float> StatValues = new();

        public bool ReadOnly = true; //Should this class only be responsible for returning values? Used to allow stat networking where only one client has control.
        
        public void ReplaceStatValueDictionary(Stat[] keys, float[] values)
        {
            if (!ReadOnly)
            {
                Debug.LogWarning("This method should only be called for ReadOnly stats for updating over the network!");
                return;
            }
            
            var newDictionary = new Dictionary<Stat, float>();
            for (var i = 0; i < keys.Length; i++)
            {
                newDictionary.Add(keys[i], values[i]);
            }
            StatValues = newDictionary;
        }
        
        public float GetStat(Stat stat)
        {
            if (!ReadOnly) RemoveExpiredBuffs();

            return StatValues.GetValueOrDefault(stat, 0);
        }

        private void UpdateStatValue(Stat stat)
        {
            if (ReadOnly) return;
            
            StatValues[stat] = GetStatValue(stat);
        }

        private Dictionary<Stat, bool> _statValueCached = new();
        
        private float GetStatValue(Stat stat)
        {
            if (ReadOnly) return 0;

            var statList = _activeStatBonuses.GetValueOrDefault(stat, null);

            if (statList == null) return 0;
            
            
            //TODO: place override logic in merging logic to allow caching the override
            //Check if there is an override for the given stat
            var overrideBonus = statList.Where(s => s.statType == StatType.Override)
                .ToArray();
            if (overrideBonus.Any()) return overrideBonus.FirstOrDefault().strength;
            //

            if (!_statValueCached.ContainsKey(stat) || !_statValueCached[stat])
            {
                StatValues[stat] = CalculateStatValue(statList);
                _statValueCached[stat] = true;
            }
            
            return StatValues[stat];
        }

        //Combines similar stats while doing calculations between the stat types. Returns a dictionary with combined value of each unique stat.
        private static float CalculateStatValue(List<StatBonus> statBonuses)
        {
            // Dictionary<Stat, float> bonusDictionary = new();
            //
            // var statBonusEnumerable = statBonuses.ToList();
            // foreach (var stat in statBonusEnumerable.Select(s => s.stat).Distinct())
            // {
            //     var flatBonuses = statBonusEnumerable.Where(s => s.statType == StatType.Flat && s.stat == stat)
            //         .ToList();
            //     var flatBonus = flatBonuses.Any() ? flatBonuses.Sum(s => s.strength) : 0;
            //
            //     var additivePercentBonuses = statBonusEnumerable
            //         .Where(s => s.statType == StatType.AdditivePercent && s.stat == stat).ToList();
            //     var additivePercentBonus =
            //         additivePercentBonuses.Any() ? additivePercentBonuses.Sum(s => s.strength) : 0;
            //
            //     var multiplyBonuses = statBonusEnumerable
            //         .Where(s => s.statType == StatType.Multiplicative && s.stat == stat).ToList();
            //     var multiply = multiplyBonuses.Any()
            //         ? multiplyBonuses
            //             .Select(s => s.strength)
            //             .Aggregate((x, y) => x * y)
            //         : 1;
            //
            //     bonusDictionary[stat] = flatBonus * (1 + additivePercentBonus) * multiply;
            // }
            //
            // return bonusDictionary;

            float flatBonus = 0;
            float additivePercentBonus = 0;
            float multiplyBonus = 1;
            
            foreach (var sb in statBonuses)
            {
                switch (sb.statType)
                {
                    case StatType.Flat:
                        flatBonus += sb.strength;
                        break;
                    case StatType.AdditivePercent:
                        additivePercentBonus += sb.strength;
                        break;
                    case StatType.Multiplicative:
                        multiplyBonus *= sb.strength;
                        break;
                    case StatType.Override:
                        return sb.strength;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return flatBonus * (1 + additivePercentBonus) * multiplyBonus;
        }

        public void
            AddStatBonus(StatType statType, Stat stat, float strength, string source, bool hidden = false) //Adds a stat bonus to the list.
        {
            if (ReadOnly) return;
            
            if(!_activeStatBonuses.ContainsKey(stat)) _activeStatBonuses[stat] = new List<StatBonus>();
            
            _activeStatBonuses[stat].Add(new StatBonus(statType, stat, strength, source, hidden));
            _statValueCached[stat] = false;
            
            OnStatChanged?.Invoke(stat);
        }

        public void ForceInvokeStatsChanged()
        {
            foreach (var stat in Enum.GetValues(typeof(Stat)))
            {
                OnStatChanged?.Invoke((Stat)stat);
            }
        }

        public void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            if (ReadOnly) return;

            var statCopy = _activeStatBonuses.Keys.ToList();
            
            foreach (var stat in statCopy)
            {
                var didAlterStat = false;

                var statBonusListCopy = _activeStatBonuses[stat].ToList();
                
                foreach (var sb in statBonusListCopy)
                {
                    if (sb.source != source) continue;
                    
                    _activeStatBonuses[stat].Remove(sb);
                    _statValueCached[stat] = false;
                    didAlterStat = true;
                }
                
                if(didAlterStat) OnStatChanged?.Invoke(stat);
            }
        }

        public void ClearStatBonuses()
        {
            if (ReadOnly) return;

            IEnumerable<Stat> statsCleared = _activeStatBonuses.Keys;
            
            _activeStatBonuses.Clear();
            
            foreach(var s in statsCleared)
                OnStatChanged?.Invoke(s);
        }

        public IEnumerable<StatBonus>
            GetStatBonuses(Stat stat,
                bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            if (ReadOnly) return null;

            if (!_activeStatBonuses.ContainsKey(stat) && returnEmptyBonus)
                return new[] { new StatBonus(StatType.Flat, stat, 0, "Empty Bonus") };

            return _activeStatBonuses[stat];
        }

        #endregion

        #region Buffs

        private readonly Dictionary<string, float>
            _buffTimers = new(); //string is the source of the buff, float is the Time.time that the buff will expire

        private int _buffIndex = 0; //Used to ensure unique buff source IDs
        
        private readonly SortedDictionary<float, HashSet<string>> _buffExpireTimes = new();

        public void AddBuff(StatBonus bonus, float time)
        {
            if (ReadOnly) return;
            
            var source = bonus.source + "_BUFF" + _buffIndex;
            _buffIndex++;

            var expirationTime = Time.time + time;

            _buffTimers[source] = expirationTime;
            
            if (!_buffExpireTimes.ContainsKey(expirationTime))
            {
                _buffExpireTimes[expirationTime] = new HashSet<string>();
            }
            _buffExpireTimes[expirationTime].Add(source);

            AddStatBonus(bonus.statType, bonus.stat, bonus.strength, source);
            OnStatChanged?.Invoke(bonus.stat);
        }
        
        public void RemoveExpiredBuffs()
        {
            var curTime = Time.time;
            
            var keysToRemove = new List<float>();
            
            foreach (var pair in _buffExpireTimes)
            {
                if (pair.Key > curTime)
                {
                    break;
                }
                
                keysToRemove.Add(pair.Key);
                
                RemoveBuffs(pair.Value);
            }

            foreach (var timeToRemove in keysToRemove)
            {
                _buffExpireTimes.Remove(timeToRemove);
            }
        }

        public void RemoveBuffs(HashSet<string> buffIDs)
        {
            if (ReadOnly) return;
            
            foreach (var bid in buffIDs)
            {
                var wasInDict = _buffTimers.Remove(bid);
                if(wasInDict) RemoveStatBonuses(bid);
            }
        }
        
        public void RemoveBuffsOfSource(string source)
        {
            if (ReadOnly) return;

            HashSet<string> removeBuffs = new();
            
            foreach (var src in _buffTimers.Keys)
            {
                if(!src.StartsWith(source + "_BUFF")) continue;

                removeBuffs.Add(src);
            }

            RemoveBuffs(removeBuffs);
        }

        #endregion
    }
}