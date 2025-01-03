using System.Linq;
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
        public float Frequency = 1f;
        public float Damage = 1f;

        private float _timer;
        
        public bool Expired => Ticks <= 0;

        public static float GetPoisonDamage(GameEntity gameEntity) => gameEntity.EffectsReadonly.OfType<PoisonEffect>().Sum(pe => pe.RemainingDamage());

        public float RemainingDamage()
        {
            return Ticks * Damage;
        }

        public override void OnUpdateOwner()
        {
            _timer += Time.deltaTime;

            if (_timer > Frequency)
            {
                Entity.CmdDamageEntity(Damage, false, null, true, DamageType.DoT);
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
