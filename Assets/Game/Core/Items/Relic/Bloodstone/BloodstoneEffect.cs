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

    public class BloodstoneAbility : GameEffect
    {
        public int Radius;
        public float DamageMult;

        public override void OnAddOwner()
        {
            Entity.OnHurtOwner += OnTakeDamage;
        }
        
        private void OnTakeDamage(float dmg)
        {
            if (dmg <= 0) return;
            
            OnAbilityActivate(dmg);
        }
        
        protected void OnAbilityActivate(float dmgTaken)
        {
            // Sacrifice a portion of the player's current health in exchange for a burst of damage.
            var damage = dmgTaken * DamageMult;

            Object.Instantiate(ObjectFinder.GetPrefab("bloodstone"), Entity.transform.position, Quaternion.identity);
            
            foreach (var ge in GameManager.EntitiesReadonly)
            {
                if (ge.Team == Entity.Team ||
                    !(Vector2.Distance(ge.transform.position, Entity.transform.position) < Radius)) continue;

                var crit = Entity.Stats.CritSuccessful;
                ge.CmdDamageEntity(crit ? damage * 2 : damage, crit, null, false, DamageType.Default);
            }
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>When taking damage, retaliate with <color=yellow>{DamageMult:P0}</color> of the damage within {Radius} tiles.</color>";
        }
    }
}