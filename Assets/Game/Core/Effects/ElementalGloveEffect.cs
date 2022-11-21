using System;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Effects
{
    public class ElementalGloveEffect : AbilityEffect
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

        private Effect _activeEffect;

        private ParticleTrailEffect _trailEffect;

        public override void OnAddOwner()
        {
            _trailEffect = new ParticleTrailEffect();
            Entity.CmdAddEffect(_trailEffect, Entity);
            _trailEffect.SetVisible(false);

            Entity.OnDamageOther += TryLeech;

            base.OnAddOwner();
        }

        public override void OnRemoveOwner()
        {
            Entity.CmdRemoveEffect(_trailEffect);
            base.OnRemoveOwner();
        }

        private SpeedEffect _speedEffect;
        private PoisonEffect _burnEffect;

        protected override void OnAbilityActivate()
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
                    _trailEffect.SetColor(Color.red);
                    break;
                case Effect.Slow:
                    Entity.OnDamageOther += InflictSlow;
                    _trailEffect.SetColor(Color.cyan);
                    break;
                case Effect.Leech:
                    _trailEffect.SetColor(Color.green);
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

        private void InflictBurn(float dmg, GameEntity other, bool crit)
        {
            _burnEffect = new PoisonEffect
            {
                Damage = BurnDamagePerSecondMultiplier * Entity.Stats.GetStat(Stat.Damage) * Frequency,
                Frequency = Frequency,
                Ticks = Ticks
            };

            other.CmdAddEffect(_burnEffect, Entity);
        }

        private void InflictSlow(float dmg, GameEntity other, bool crit)
        {
            other.CmdRemoveEffect(_speedEffect);
            
            _speedEffect = new SpeedEffect
            {
                Duration = SlowDuration,
                MoveSpeedMultiplier = 1 - SlowAmount,
                Source = Entity
            };

            other.CmdAddEffect(_speedEffect, Entity);
        }

        protected void TryLeech(float dmg, GameEntity other, bool isCrit)
        {
            if (_activeEffect == Effect.Leech)
            {
                var leech = -dmg * LeechPercent;

                ObjectSpawner.SpawnDmgPopup(Entity.transform.position, leech, isCrit);

                Entity.ApplyDamage(leech, out var success);
            }
        }

        protected override void OnAbilityDeactivate()
        {
            _trailEffect.SetVisible(false);
            ClearEffects();
        }
    }
}