using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev
{
    public class SlowWhileClickingEffect : GameEffect
    {
        public float SpeedMultiplier = 0.7f;
        
        private double _nextUpdateTime;
        
        public override void OnUpdateOwner()
        {
            if (Time.time > _nextUpdateTime && Player.LocalInstance.fireKeyHeld)
            {
                _nextUpdateTime = Time.time + 0.1f;
                Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, SpeedMultiplier, EffectIdentifier), 0.1f);
            }
        }
    }
}
