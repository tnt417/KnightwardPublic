using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class RedStimulantsEffect : GameEffect
    {
        public float healthRedirectMultiplier;
        
        public override void OnAddOwner()
        {
            
        }

        public override void OnRemoveOwner()
        {
            
        }

        private float _nextBuffTime;
        
        public override void OnUpdateOwner()
        {
            if (_nextBuffTime > Time.time) return;
            
            _nextBuffTime = Time.time + 1f;

            var strength = healthRedirectMultiplier * Entity.Stats.GetStat(Stat.Health);
            
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Damage, strength, "RedStimulants"), 1f);
        }
    }
}