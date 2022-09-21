using System.Runtime.Serialization;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PoisonEffect : GameEffect
    {
        public int Ticks = 10;
        public float Frequency = 0.5f;
        public float Damage = 1f;

        private float _timer;
        
        public override void OnAddOwner()
        {
            Damage = Source.Stats.GetStat(Stat.Damage) / 10f;
        }

        public override void OnRemoveOwner()
        {
            
        }

        public override void OnUpdateOwner()
        {
            _timer += Time.deltaTime;

            if (_timer > Frequency)
            {
                Entity.CmdDamageEntity(Damage, false, null);
                _timer = 0f;
                
                Ticks--;
                
                if (Ticks <= 0)
                {
                    Entity.CmdRemoveEffect(this);
                }
            }
        }
    }
}
