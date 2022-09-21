using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class SpeedEffect : GameEffect
    {
        public float moveSpeedMultiplier;
        public float duration;

        private float expireTime;
        
        public override void OnAddOwner()
        {
            expireTime = Time.time + duration;
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, moveSpeedMultiplier, "SlowEffect"), duration);
        }

        public override void OnRemoveOwner()
        {
            
        }

        public override void OnUpdateOwner()
        {
            if (Time.time > expireTime)
            {
                Entity.CmdRemoveEffect(this);
            }
        }
    }
}