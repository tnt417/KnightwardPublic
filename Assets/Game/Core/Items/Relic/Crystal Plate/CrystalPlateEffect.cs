using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class CrystalPlateEffect : GameEffect
    {
        public Vector2 DamageProportionRange;
        public float PercentHealthThreshold;

        public ProjectileData ProjectileData;

        private float DamageProportion => LinearScale(DamageProportionRange.x, DamageProportionRange.y, 50);

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
                ProjectileData.attackData.damageMultiplier = _stored * DamageProportion / Entity.Stats.GetStat(Stat.Damage);
                ObjectSpawner.SpawnProjectile(Entity, Entity.transform.position, Vector2.zero, ProjectileData, true);
                _stored = 0;
            }
        }

        public override void OnRemoveOwner()
        {
            Entity.OnHurtOwner -= StoreDamage;
        }

        public override string GetEffectDescription()
        {
            return
                $"When armor reduces incoming damage, store the amount. Once this amount reaches {Tools.WrapColor($"{PercentHealthThreshold:P0}", Color.yellow)}" +
                $" of your maximum health, fire a homing projectile, dealing {Tools.WrapColor($"{DamageProportion:P0}", Color.yellow)} of this damage amount.";
        }
    }
}
