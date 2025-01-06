using System.Collections.Generic;
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

        public static Dictionary<GameEntity, ParticleTrailEffect> PoisonTrailEffects = new();

        private ParticleTrailEffect _myParticleTrailEffect;
        
        public static float GetPoisonDamage(GameEntity gameEntity) => gameEntity.EffectsReadonly.OfType<PoisonEffect>().Sum(pe => pe.RemainingDamage());

        public override void OnAddClient()
        {
            _myParticleTrailEffect = new ParticleTrailEffect()
            {
                OverridePrefab = "poisonParticles"
            };
            
            if (PoisonTrailEffects.TryAdd(Entity, _myParticleTrailEffect))
            {
                Entity.CmdAddEffect(_myParticleTrailEffect, Entity);
            }
            else
            {
                _myParticleTrailEffect = PoisonTrailEffects[Entity];
            }
        }
        
        public override void OnRemoveClient()
        {
            if (GetPoisonDamage(Entity) <= 0)
            {
                Entity.CmdRemoveEffect(PoisonTrailEffects[Entity]);
                PoisonTrailEffects.Remove(Entity);
            }
        }
        
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
