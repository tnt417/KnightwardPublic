using TonyDev.Game.Core.Entities;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public abstract class AbilityEffect : GameEffect
    {
        protected KeyCode ActivateButton;
        
        protected float Cooldown;
        protected float Duration;
        private float ModifiedCooldown => Cooldown * (1 - Entity.Stats.GetStat(Stat.CooldownReduce));
        
        private bool _active;

        private float _activeTimer;
        private float _cooldownTimer;
        
        protected abstract void OnAbilityActivate();

        protected abstract void OnAbilityDeactivate();
        
        public override void OnUpdate()
        {
            _cooldownTimer += Time.deltaTime;

            if (_cooldownTimer > ModifiedCooldown && Input.GetKeyDown(ActivateButton))
            {
                _active = true;
                OnAbilityActivate();
                _activeTimer = 0;
                _cooldownTimer = 0;
            }

            if (_active)
            {
                _activeTimer += Time.deltaTime;
                if (_activeTimer > Duration)
                {
                    _active = false;
                    OnAbilityDeactivate();
                }
            }
        }
    }
}
