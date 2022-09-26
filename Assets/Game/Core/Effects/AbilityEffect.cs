using System;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.UI.GameInfo;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    [Serializable]
    public class AbilityEffect : GameEffect
    {
        public KeyCode ActivateButton;

        public float Cooldown;
        public float Duration;
        public Sprite abilitySprite;
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

        private int _cooldownID;
        
        public override void OnAddOwner()
        {
            _cooldownID = CooldownUIController.RegisterCooldown(abilitySprite, () => ModifiedCooldown, () => ModifiedCooldown - _cooldownTimer, ActivateButton);
        }

        public override void OnRemoveOwner()
        {
            OnAbilityDeactivate();
            CooldownUIController.RemoveCooldown(_cooldownID);
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

        public void DiscountCooldown(float amount, bool ofTotal)
        {
            if(ofTotal) _cooldownTimer += amount * ModifiedCooldown;
            else _cooldownTimer += (ModifiedCooldown - _cooldownTimer) * amount;
        }
    }
}