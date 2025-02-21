using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class ElementalGloveEffect : GameEffect
    {
        private enum Effect
        {
            Slow,
            Burn,
            Leech
        }

        public float SlowAmount;
        public float SlowDuration;
        public float BurnDamagePerSecondMultiplier;
        public float Frequency;
        public int Ticks;
        public float LeechPercent;

        public float CycleCd;
        private float _nextCycleTime;

        private Effect _activeEffect;

        private ParticleTrailEffect _trailEffect;

        public override void OnAddOwner()
        {
            _nextCycleTime = Time.time + CycleCd;
            
            _trailEffect = new ParticleTrailEffect();
            Entity.AddEffect(_trailEffect, Entity);
            _trailEffect.SetVisible(false);

            Entity.OnDamageOther += TryLeech;

            base.OnAddOwner();
            
            CycleFx();
        }

        public override void OnRemoveOwner()
        {
            _trailEffect.SetVisible(false);
            ClearEffects();
            Entity.RemoveEffect(_trailEffect);
        }

        private SpeedEffect _speedEffect;
        private PoisonEffect _burnEffect;

        public override void OnUpdateOwner()
        {
            if (Time.time > _nextCycleTime)
            {
                CycleFx();
                _nextCycleTime = Time.time + CycleCd;
            }
        }
        
        protected void CycleFx()
        {
            _activeEffect = 
                _activeEffect switch
                {
                    Effect.Burn => Effect.Slow,
                    Effect.Slow => Effect.Leech,
                    Effect.Leech => Effect.Burn,
                    _ => Effect.Slow
                };
            
            _trailEffect.SetVisible(true);

            ClearEffects();
            
            switch (_activeEffect)
            {
                case Effect.Burn:
                    Entity.OnDamageOther += InflictBurn;
                    _trailEffect.SetColor(new Color(230f/255f, 69f/255f, 57f/255f));
                    break;
                case Effect.Slow:
                    Entity.OnDamageOther += InflictSlow;
                    _trailEffect.SetColor(new Color(146f/255f, 232f/255f, 192f/255f));
                    break;
                case Effect.Leech:
                    _trailEffect.SetColor(new Color(200f/255f, 212f/255f, 93f/255f));
                    Entity.OnDamageOther += TryLeech;
                    break;
            }
        }

        private void ClearEffects()
        {
            Entity.OnDamageOther -= InflictBurn;
            Entity.OnDamageOther -= InflictSlow;
            Entity.OnDamageOther -= TryLeech;
        }

        private void InflictBurn(float dmg, GameEntity other, bool crit, DamageType dt)
        {
            if (dmg <= 0) return;
            
            _burnEffect = new PoisonEffect
            {
                Damage = BurnDamagePerSecondMultiplier * Entity.Stats.GetStat(Stat.Damage) * Frequency,
                Frequency = Frequency,
                Ticks = Ticks
            };

            other.AddEffect(_burnEffect, Entity);
        }

        private void InflictSlow(float dmg, GameEntity other, bool crit, DamageType dt)
        {
            if (dmg <= 0) return;
            
            other.RemoveEffect(_speedEffect);
            
            _speedEffect = new SpeedEffect
            {
                Duration = SlowDuration,
                MoveSpeedMultiplier = 1 - SlowAmount,
                Source = Entity
            };

            other.AddEffect(_speedEffect, Entity);
        }

        protected void TryLeech(float dmg, GameEntity other, bool isCrit, DamageType dt)
        {
            if (dmg <= 0) return;
            
            if (_activeEffect == Effect.Leech)
            {
                var leech = -dmg * LeechPercent;

                ObjectSpawner.SpawnDmgPopup(Entity.transform.position, leech, isCrit, DamageType.Heal);

                Entity.ApplyDamage(leech, out var success);
            }
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Gain elemental effects that cycle every {GameTools.WrapColor($"{CycleCd:N0}", Color.yellow)} seconds.</color>";
        }
    }
}