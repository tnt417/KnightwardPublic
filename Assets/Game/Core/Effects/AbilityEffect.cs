using System;
using TonyDev.Game.Core.Entities;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    [Serializable]
    public class AbilityEffect : GameEffect
    {
        protected KeyCode ActivateButton;

        public float Cooldown;
        public float Duration;
        private float ModifiedCooldown => Cooldown * (1 - Entity.Stats.GetStat(Stat.CooldownReduce));

        protected bool Active { get; private set; }

        private float _activeTimer;
        private float _cooldownTimer;

        protected virtual void OnAbilityActivate()
        {
        }

        protected virtual void OnAbilityDeactivate()
        {
        }

        public override void OnUpdateOwner()
        {
            _cooldownTimer += Time.deltaTime;

            if (_cooldownTimer > ModifiedCooldown && Input.GetKeyDown(ActivateButton))
            {
                Active = true;
                OnAbilityActivate();
                _activeTimer = 0;
                _cooldownTimer = 0;
            }

            if (Active)
            {
                _activeTimer += Time.deltaTime;
                if (_activeTimer > Duration)
                {
                    Active = false;
                    OnAbilityDeactivate();
                }
            }
        }
    }
}