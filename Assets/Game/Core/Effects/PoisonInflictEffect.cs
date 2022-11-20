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

        private PoisonEffect _lastInflictedEffect;
        
        public void InflictPoison(float dmg, GameEntity entity, bool crit)
        {
            if (DoPoisonStacking || _lastInflictedEffect is {Expired: true} or null)
            {
                _lastInflictedEffect = new PoisonEffect()
                {
                    Damage = DmgMult * Entity.Stats.GetStat(Stat.Damage) / TickCount,
                    Ticks = TickCount,
                    Frequency = TickFreq
                };
                
                entity.CmdAddEffect(_lastInflictedEffect, Entity);
            }
            else
            {
                _lastInflictedEffect.Ticks = TickCount;
                _lastInflictedEffect.Frequency = TickFreq;
                _lastInflictedEffect.Damage = DmgMult * Entity.Stats.GetStat(Stat.Damage) / TickCount;
            }
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= InflictPoison;
        }
    }
}