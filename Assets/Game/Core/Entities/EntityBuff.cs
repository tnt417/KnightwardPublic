using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev
{
    public class EntityBuff
    {
        public List<StatBonus> _activeStatBonuses = new ();
        
        public delegate void StatAction();

        public static event StatAction OnStatsChanged;

        public void
            AddStatBonus(StatType statType, Stat stat, float strength, string source) //Adds a stat bonus to the list.
        {
            _activeStatBonuses.Add(new StatBonus(statType, stat, strength, source));
            OnStatsChanged?.Invoke();
        }

        public void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
        {
            _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
            OnStatsChanged?.Invoke();
        }

        public void ClearStatBonuses()
        {
            _activeStatBonuses.Clear();
            OnStatsChanged?.Invoke();
        }

        public float GetStatMultiplyBonus(Stat stat)
        {
            return 1 + _activeStatBonuses.Where(sb => sb.statType == StatType.Multiplicative && sb.stat == stat)
                .Sum(sb => sb.strength);
        }

        public float GetFlatStatBonus(Stat stat) //Returns a sum of all stat bonuses of a certain type.
        {
            return _activeStatBonuses.Where(sb => sb.statType == StatType.Flat && sb.stat == stat)
                .Sum(sb => sb.strength);
        }

        private IEnumerable<StatBonus>
            GetStatBonuses(Stat stat,
                bool returnEmptyBonus) //Returns an array of stat bonuses of stat specified. If returnEmptyBonus is true and no bonuses are found, empty ones will be created.
        {
            var foundBonuses = _activeStatBonuses.Where(sb => sb.stat == stat).ToArray();
            if (foundBonuses.Length != 0) return foundBonuses;
            return returnEmptyBonus ? new[] {new StatBonus(StatType.Flat, stat, 0, "Empty Bonus")} : foundBonuses;
        }
    }
}