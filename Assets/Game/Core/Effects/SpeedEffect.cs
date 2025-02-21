using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class SpeedEffect : GameEffect
    {
        public float MoveSpeedMultiplier;
        public float Duration;
        private float _expireTime;
        
        public override void OnAddOwner()
        {
            _expireTime = Time.time + Duration;
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, MoveSpeedMultiplier, EffectIdentifier), Duration);
        }

        public override void OnRemoveOwner()
        {
            Entity.Stats.RemoveBuffsOfSource(EffectIdentifier);
        }

        public override void OnUpdateOwner()
        {
            if (Time.time > _expireTime)
            {
                Entity.RemoveEffect(this);
            }
        }
    }
}