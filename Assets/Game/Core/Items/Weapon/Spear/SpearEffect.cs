using System;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [Serializable]
    public class SpearEffect : AbilityEffect
    {
        public float MoveSpeedMultiplier;
        public float AttackSpeedMultiplier;
        public float AoeMultiplier;
        public float PoisonDPSMultiplier;

        public ProjectileData regularProjectile;
        public ProjectileData empoweredProjectile;

        private bool _empowered;

        public override void OnAddOwner()
        {
            empoweredProjectile.OnHitOther += InflictPoison;
            Entity.OnAttack += Shoot;
            base.OnAddOwner();
        }

        public void Shoot()
        {
            var spawnPos = Entity.transform.position;
            var direction = GameManager.MouseDirection;

            SoundManager.PlaySoundPitchVariant("spear", spawnPos, 0.8f, 1.2f);

            if (_empowered)
            {
                var direction1 = Tools.Rotate(direction, -10 * Mathf.Deg2Rad);
                var direction2 = Tools.Rotate(direction, 10 * Mathf.Deg2Rad);
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction, empoweredProjectile);
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction1, empoweredProjectile);
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction2, empoweredProjectile);
            }
            else
            {
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction, regularProjectile);
            }
        }

        public override void OnRemoveOwner()
        {
            empoweredProjectile.OnHitOther -= InflictPoison;
            Entity.OnAttack -= Shoot;
            base.OnRemoveOwner();
        }

        public void InflictPoison(float dmg, GameEntity entity, bool crit)
        {
            if (entity == null) return;
            
            entity.CmdAddEffect(new PoisonEffect()
            {
                Damage = PoisonDPSMultiplier * Entity.Stats.GetStat(Stat.Damage),
                Ticks = 10,
                Frequency = 1f,
            }, Entity);
        }

        protected override void OnAbilityActivate()
        {
            _empowered = true;

            Entity.Stats.AddBuff(
                new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, MoveSpeedMultiplier, "spearEffect"), Duration);
            Entity.Stats.AddBuff(
                new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, AttackSpeedMultiplier, "spearEffect"),
                Duration);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.AoeSize, AoeMultiplier, "spearEffect"),
                Duration);
        }

        protected override void OnAbilityDeactivate()
        {
            _empowered = false;
        }
    }
}