using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class HeavenlyGazeEffect : AbilityEffect
    {
        public Vector2 DamageMultRange;
        private float DamageMult => LinearScale(DamageMultRange.x, DamageMultRange.y, 50);
        
        // The GameEntity of the highlighted enemy
        private GameEntity _highlightedEnemy;

        private ParticleTrailEffect _vfx;
        
        protected override void OnAbilityActivate()
        {
            if(_highlightedEnemy != null) _highlightedEnemy.CmdRemoveEffect(_vfx);
            
            // Find the nearest enemy and highlight it
            _highlightedEnemy = GameManager.GetEntitiesInRange(Entity.transform.position, 10f).First(ge => ge.Team != Entity.Team);
            _highlightedEnemy.CmdAddEffect(_vfx = new ParticleTrailEffect(), Entity);

            // Increase the player's next attack damage against the highlighted enemy
            Entity.OnDamageOther += IncreaseDamage;
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            // Remove the highlight from the enemy and the OnNextAttack event
            if(_vfx != null) _highlightedEnemy.CmdRemoveEffect(_vfx);
            Entity.OnDamageOther -= IncreaseDamage;
        }

        // Increase the player's next attack damage against the highlighted enemy
        private void IncreaseDamage(float damage, GameEntity target, bool wasCrit)
        {
            if (target == _highlightedEnemy)
            {
                target.CmdDamageEntity(damage * (DamageMult-1) * (wasCrit ? 2 : 1), wasCrit, null, false);
            }
            
            if(_highlightedEnemy != null) _highlightedEnemy.CmdRemoveEffect(_vfx);

            Entity.OnDamageOther -= IncreaseDamage;
        }
        
        public override string GetEffectDescription()
        {
            return $"<color=green>Upon activation, highlights one nearby enemy and makes the next attack on that enemy deal <color=yellow>{DamageMult:N0}</color> times as much damage.</color>";
        }
    }
}
