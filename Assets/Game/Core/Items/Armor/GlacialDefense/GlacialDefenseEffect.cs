using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class GlacialDefenseEffect : GameEffect
    {
        public string SlowEffectKey;
        public string FrozenEffectKey;
        public float TickFrequency = 0.5f;
        public float TickStrength = 0.2f;
        public float Range = 5f;
        public float BonusDamageMult = 2f;

        private Dictionary<GameEntity, ParticleTrailEffect> _slowedEntities = new();
        private Dictionary<GameEntity, ParticleTrailEffect> _frozenEntities = new();

        public override void OnAddOwner()
        {
            Entity.OnDamageOther += OnHit;
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= OnHit;
        }

        private void OnHit(float damage, GameEntity ge, bool crit, DamageType dt)
        {
            if (ge == null) return;

            if (_frozenEntities.ContainsKey(ge))
            {
                ge.RemoveEffect(_frozenEntities[ge]);
                _frozenEntities.Remove(ge);

                ge.CmdRemoveBonusesFromSource(EffectIdentifier);
                ge.CmdDamageEntity(damage * BonusDamageMult, crit, null, true, DamageType.Default);
            }
        }

        private float _timer;

        public override void OnUpdateServer()
        {
            _timer += Time.deltaTime;

            if (_timer < TickFrequency) return;

            _timer = 0;

            foreach (var ge in GameManager.GetEntitiesInRange(Entity.transform.position, Range)
                .Where(ge => ge.Team != Entity.Team))
            {
                if (ge.Stats.GetStat(Stat.MoveSpeed) <= 0) continue;
                if (ge.Stats.GetStatBonuses(Stat.MoveSpeed, false).Count(sb => sb.source == EffectIdentifier) >=
                    3) continue;
                ge.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, -TickStrength, EffectIdentifier);
            }
        }

        public override void OnUpdateOwner()
        {
            foreach (var ge in GameManager.GetEntitiesInRange(Entity.transform.position, Range)
                .Where(ge => ge.Team != Entity.Team))
            {
                if (!_slowedEntities.ContainsKey(ge) && !_frozenEntities.ContainsKey(ge))
                {
                    var trail = new ParticleTrailEffect()
                    {
                        OverridePrefab = SlowEffectKey
                    };
                    ge.AddEffect(trail, Entity);
                    _slowedEntities.Add(ge, trail);
                }
                else if (!_frozenEntities.ContainsKey(ge) && ge.Stats.GetStatBonuses(Stat.MoveSpeed, false)
                        .Count(sb => sb.source == EffectIdentifier) >= 3)
                {
                    ge.RemoveEffect(_slowedEntities[ge]);

                    var trail = new ParticleTrailEffect()
                    {
                        OverridePrefab = FrozenEffectKey
                    };

                    ge.AddEffect(trail, Entity);

                    _slowedEntities.Remove(ge);
                    _frozenEntities.Add(ge, trail);
                }
            }
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Freeze enemies near you over time, slowing them. Deal extra damage to frozen enemies, but unfreeze them in the process.</color>";
        }
    }
}