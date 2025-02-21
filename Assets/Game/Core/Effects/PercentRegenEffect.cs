using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev
{
    public class PercentRegenEffect : GameEffect
    {
        public float PercentRegen;
        private float _timer;
        public override void OnUpdateOwner()
        {
            _timer += Time.deltaTime;

            if (_timer < 0.5f) return;
            
            _timer = 0;
            
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.HpRegen, Entity.Stats.GetStat(Stat.Health) * PercentRegen, EffectIdentifier), 0.5f);
        }
    }
}
