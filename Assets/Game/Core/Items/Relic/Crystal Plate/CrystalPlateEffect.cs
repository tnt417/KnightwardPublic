using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class 
        CrystalPlateEffect : GameEffect
    {
        public Vector2 DamageProportionRange;
        public float PercentHealthThreshold;
        public Vector2 ProjectileAmountRange;

        public ProjectileData ProjectileData;

        private float DamageProportion => LinearScale(DamageProportionRange.x, DamageProportionRange.y, 50);
        private int Projectiles => (int)LinearScale(ProjectileAmountRange.x, ProjectileAmountRange.y, 50);

        public override void OnAddOwner()
        {
            Entity.OnHurtOwner += StoreDamage;
        }

        private float _stored;
        
        private void StoreDamage(float hp)
        {
            var preMitigation = hp / (100f / (100 + Entity.Stats.GetStat(Stat.Armor)));

            _stored += preMitigation - hp;

            if (_stored >= PercentHealthThreshold * Entity.NetworkMaxHealth)
            {
                Shoot().Forget();
            }
        }

        private async UniTask Shoot()
        {
            ProjectileData.attackData.damageMultiplier =
                _stored * DamageProportion / Entity.Stats.GetStat(Stat.Damage);
            for (int i = 0; i < Projectiles; i++)
            {
                ObjectSpawner.SpawnProjectile(Entity, Entity.transform.position, Vector2.zero, ProjectileData,
                    true);
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            }
            _stored = 0;
        }

        public override void OnRemoveOwner()
        {
            Entity.OnHurtOwner -= StoreDamage;
        }

        public override string GetEffectDescription()
        {
            return
                $"When armor reduces incoming damage, store the amount. Once this amount reaches {GameTools.WrapColor($"{PercentHealthThreshold:P0}", Color.yellow)}" +
                $" of your maximum health, fire <color=yellow>{Projectiles}</color> homing projectile(s), each dealing {GameTools.WrapColor($"{DamageProportion:P0}", Color.yellow)} of this damage amount.";
        }
    }
}
