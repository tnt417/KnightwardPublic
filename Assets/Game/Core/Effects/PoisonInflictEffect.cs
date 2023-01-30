using System.Collections.Generic;
using TonyDev.Game.Core.Entities;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PoisonInflictEffect : GameEffect
    {
        public Vector2 DamageMultiplierScale;
        private float DmgMult => LinearScale(DamageMultiplierScale.x, DamageMultiplierScale.y, 50);
        public Vector2 TickFrequencyScale;
        private float TickFreq => LinearScale(TickFrequencyScale.x, TickFrequencyScale.y, 50);
        public int TickCount;
        public bool DoPoisonStacking;
        
        public override void OnAddOwner()
        {
            Entity.OnDamageOther += InflictPoison;
        }

        private readonly Dictionary<GameEntity, PoisonEffect> _lastInflictedEffects = new();
        
        public void InflictPoison(float dmg, GameEntity entity, bool crit)
        {
            if (entity.Team == Entity.Team) return;
            
            if (DoPoisonStacking || (_lastInflictedEffects.ContainsKey(entity) && _lastInflictedEffects[entity] is null or {Expired: true}) || !_lastInflictedEffects.ContainsKey(entity))
            {
                _lastInflictedEffects[entity] = new PoisonEffect()
                {
                    Damage = DmgMult * Entity.Stats.GetStat(Stat.Damage) / TickCount,
                    Ticks = TickCount,
                    Frequency = TickFreq
                };
                
                entity.CmdAddEffect(_lastInflictedEffects[entity], Entity);
            }
            else
            {
                _lastInflictedEffects[entity].Ticks = TickCount;
                _lastInflictedEffects[entity].Frequency = TickFreq;
                _lastInflictedEffects[entity].Damage = DmgMult * Entity.Stats.GetStat(Stat.Damage) / TickCount;
            }
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= InflictPoison;
        }

        public override string GetEffectDescription()
        {
            return $"Upon damaging an enemy, deal <color=yellow>{DmgMult:P0}</color> of your damage stat over <color=yellow>{(TickFreq * TickCount):N1}</color> seconds. This does not stack.";
        }
    }
}