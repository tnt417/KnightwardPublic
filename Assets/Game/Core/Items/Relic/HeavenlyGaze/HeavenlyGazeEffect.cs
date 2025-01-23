using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class HeavenlyGazeEffect : GameEffect
    {
        public Vector2 DamageMultRange;
        private float DamageMult => LinearScale(DamageMultRange.x, DamageMultRange.y, 50);

        public float HighlightCooldown;
        private float _nextHighlightTime;
        
        // The GameEntity of the highlighted enemy
        private GameEntity _highlightedEnemy;

        private ParticleTrailEffect _vfx;

        public override void OnUpdateOwner()
        {
            if (Time.time > _nextHighlightTime)
            {
                OnAbilityActivate();
                _nextHighlightTime = Time.time + HighlightCooldown;
            }
        }
        
        protected void OnAbilityActivate()
        {
            // Find the nearest enemy and highlight it
            var newHighlightedEnemy = GameManager.EntitiesReadonly.Where(ge => ge.Team != Entity.Team && ge.CurrentParentIdentity == Entity.CurrentParentIdentity).OrderByDescending(ge => ge.NetworkCurrentHealth).FirstOrDefault();

            if (_highlightedEnemy == newHighlightedEnemy) return;

            if (_highlightedEnemy != null)
            {
                _highlightedEnemy.RemoveEffect(_vfx);
            }

            if (newHighlightedEnemy == null) return;

            _highlightedEnemy = newHighlightedEnemy;

            _highlightedEnemy.AddEffect(_vfx = new ParticleTrailEffect(), Entity);

            // Increase the player's next attack damage against the highlighted enemy
            Entity.OnDamageOther += IncreaseDamage;
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            // Remove the highlight from the enemy and the OnNextAttack event
            if(_vfx != null) _highlightedEnemy.RemoveEffect(_vfx);
            Entity.OnDamageOther -= IncreaseDamage;
        }

        // Increase the player's next attack damage against the highlighted enemy
        private void IncreaseDamage(float damage, GameEntity target, bool wasCrit, DamageType dt)
        {
            if (dt == DamageType.DoT) return;
            
            if (target == _highlightedEnemy)
            {
                target.CmdDamageEntity(damage * (DamageMult-1) * (wasCrit ? 2 : 1), wasCrit, null, false, DamageType.Default);
            }
            
            if(_highlightedEnemy != null) _highlightedEnemy.RemoveEffect(_vfx);

            Entity.OnDamageOther -= IncreaseDamage;
        }
        
        public override string GetEffectDescription()
        {
            return $"<color=#63ab3f>Every {GameTools.WrapColor($"{HighlightCooldown:N0}", Color.yellow)} seconds, highlight the enemy with the highest health in your instance. Your next attack on the highlighted enemy deals <color=yellow>{DamageMult:N0}</color> times as much damage.</color>";
        }
    }
}
