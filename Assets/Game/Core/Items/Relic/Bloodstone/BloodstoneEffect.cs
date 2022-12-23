using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class BloodstoneEffect : GameEffect
    {
        public Vector2 PercentageScaling;
        public float Percentage => LinearScale(PercentageScaling.x, PercentageScaling.y, 50);

        private float _buffTimer;

        public override void OnUpdateOwner()
        {
            _buffTimer += Time.deltaTime;

            if (_buffTimer >= 0.25f)
            {
                var maxHP = Entity.MaxHealth;
                // Increase the player's damage by a percentage of their missing health.
                Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Damage,
                    maxHP * Percentage * (1 - Entity.CurrentHealth / maxHP), "Bloodstone"), 0.25f);

                _buffTimer = 0;
            }
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>The player's damage is increased by <color=yellow>{Percentage:P0}</color> of their missing health.</color>";
        }
    }

    public class BloodstoneAbility : AbilityEffect
    {
        public int Radius;
        public float HealthMult;
        public Vector2 DamageMultRange;
        public float DamageMult => LinearScale(DamageMultRange.x, DamageMultRange.y, 50);

        protected override void OnAbilityActivate()
        {
            // Sacrifice a portion of the player's current health in exchange for a burst of damage.
            var damage = Entity.MaxHealth * HealthMult;

            if (Entity.CurrentHealth - damage < 0)
            {
                DiscountCooldown(1, true);
                return;
            }
            
            Entity.SetHealth(Entity.CurrentHealth - damage);

            Object.Instantiate(ObjectFinder.GetPrefab("bloodstone"), Entity.transform.position, Quaternion.identity);
            
            foreach (var ge in GameManager.EntitiesReadonly)
            {
                if (ge.Team == Entity.Team ||
                    !(Vector2.Distance(ge.transform.position, Entity.transform.position) < Radius)) continue;

                var crit = Entity.Stats.CritSuccessful;
                ge.CmdDamageEntity((crit ? damage * 2 : damage) * DamageMult, crit, null, false);
            }

            base.OnAbilityActivate();
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>Sacrifice <color=yellow>{HealthMult:P0}</color> of the player's current health in exchange for a burst of damage within {Radius} tiles equal to <color=yellow>{DamageMult:P0}</color> of the amount of health sacrificed.</color>";
        }
    }
}