using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.Stopwatch
{
    public class EnchantedStopwatchEffect : GameEffect
    {
        public Vector2 RadiusRange;
        public float Radius => DiminishingScale(RadiusRange.x, RadiusRange.y, 50);
        public Vector2 MoveStealRange;
        private float MoveStealAmount => DiminishingScale(MoveStealRange.x, MoveStealRange.y, 50);
        public Vector2 AttackStealRange;
        private float AttackStealAmount => LinearScale(AttackStealRange.x, AttackStealRange.y, 50);

        private float _buffTimer;
        
        public override void OnUpdateOwner()
        {
            _buffTimer += Time.deltaTime;

            if (_buffTimer >= 0.1f)
            {
                _buffTimer = 0f;
                
                DoStealing();
            }
        }

        private Dictionary<GameEntity, GameEffect> Trails = new();
        
        private void DoStealing()
        {
            var inRange = GameManager.GetEntitiesInRange(Entity.transform.position, Radius).Where(ge => ge.Team != Entity.Team).ToHashSet();
            
            foreach (var ge in inRange)
            {
                ge.AddEffect(new StatBuffEffect()
                {
                    Duration = 0.1f,
                    StatBonuses = new []
                    {
                        new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, 1-AttackStealAmount, EffectIdentifier),
                        new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, 1-MoveStealAmount, EffectIdentifier)
                    }
                }, Entity);

                if (Trails.ContainsKey(ge)) continue;
                
                var particleEffect = new ParticleTrailEffect()
                {
                    OverridePrefab = "stopwatchParticles",
                    VisibleGlobal = true
                };
                
                ge.AddEffect(particleEffect, Entity);
                
                Trails.Add(ge, particleEffect);
            }

            var scan = GameManager.EntitiesReadonly;

            foreach (var ge in scan.Where(ge => !inRange.Contains(ge) && Trails.ContainsKey(ge)))
            {
                ge.RemoveEffect(Trails[ge]);
                Trails.Remove(ge);
            }
            
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.AttackSpeed, inRange.Count * AttackStealAmount, EffectIdentifier), 0.1f);
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.MoveSpeed, inRange.Count * MoveStealAmount, EffectIdentifier), 0.1f);
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Steal <color=yellow>{AttackStealAmount:P0}</color> attack and <color=yellow>{MoveStealAmount:P0}</color> move speed from enemies within <color=yellow>{Radius:N1}</color> units, slowing them down while speeding you up.</color>";
        }
    }
}
