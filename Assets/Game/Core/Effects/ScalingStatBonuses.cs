using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public enum ScalingType
    {
        Linear, Diminishing
    }
    
    [Serializable]
    public class ScalingStatBonus
    {
        public StatType type;
        public Stat stat;
        public ScalingType scalingType;
        public Vector2 valueRange;
        public int byFloor;
    }
    
    public class ScalingStatBonusEffect : GameEffect
    {
        public ScalingStatBonus[] ScalingStatBonuses;

        [NonSerialized] private List<StatBonus> _modifiedBonuses = new();
        
        public override void OnAddOwner()
        {
            CalculateModifiedBonuses();

            if (_modifiedBonuses is {Count: 0} or null) return;
            
            foreach (var bonus in _modifiedBonuses)
            {
                Entity.Stats.AddStatBonus(bonus.statType, bonus.stat, bonus.strength, bonus.source);
            }
        }

        private void CalculateModifiedBonuses()
        {
            _modifiedBonuses = new List<StatBonus>();

            if (ScalingStatBonuses == null) return;
            
            foreach (var ssb in ScalingStatBonuses)
            {
                var newStrength = ssb.scalingType == ScalingType.Linear
                    ? LinearScale(ssb.valueRange.x, ssb.valueRange.y, ssb.byFloor) : DiminishingScale(ssb.valueRange.x, ssb.valueRange.y, ssb.byFloor);

                var newStatBonus = new StatBonus(ssb.type, ssb.stat, newStrength, EffectIdentifier);
                
                _modifiedBonuses.Add(newStatBonus);
            }
        }
        
        public override void OnRemoveOwner()
        {
            Entity.Stats.RemoveStatBonuses(EffectIdentifier);
        }

        public override void OnRegisterLocal()
        {
            base.OnRegisterLocal();
            
            CalculateModifiedBonuses();
        }

        public override string GetEffectDescription()
        {
            return PlayerStats.GetStatsTextFromBonuses(_modifiedBonuses, true, true);
        }
    }
}
