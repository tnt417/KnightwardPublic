using System;
using Newtonsoft.Json;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
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
        protected bool Ready => _cooldownTimer > ModifiedCooldown;
        [JsonConverter(typeof(SpriteConverter))] public Sprite abilitySprite;
        protected float ModifiedCooldown => Mathf.Clamp(Cooldown * (1 - Entity.Stats.GetStat(Stat.CooldownReduce)), 0, Mathf.Infinity);

        protected bool Active { get; private set; }

        private float _activeTimer;
        private float _cooldownTimer;

        protected virtual void OnAbilityActivate()
        {
            _cooldownTimer = 0;
        }

        protected virtual void OnAbilityDeactivate()
        {
        }

        private int _cooldownID;

        private int _abilityIndex;
        
        public override void OnAddOwner()
        {
            _abilityIndex = AbilityControlManager.Instance.GetOpenIndex();
        
            if (ActivateButton == default)
            {
                ActivateButton = _abilityIndex switch
                {
                    0 => KeyCode.Alpha1,
                    1 => KeyCode.Alpha2,
                    2 => KeyCode.Alpha3,
                    3 => KeyCode.Alpha4,
                    4 => KeyCode.Alpha5,
                    5 => KeyCode.Alpha6,
                    6 => KeyCode.Alpha7,
                    7 => KeyCode.Alpha8,
                    8 => KeyCode.Alpha9,
                    9 => KeyCode.Alpha0,
                    _ => KeyCode.None
                };
            }
            
            _cooldownID = CooldownUIController.RegisterCooldown(abilitySprite, () => ModifiedCooldown, () => ModifiedCooldown - _cooldownTimer, ActivateButton);
            AbilityControlManager.Instance.RegisterCallback(TryActivate, _abilityIndex);
        }

        public override void OnRemoveOwner()
        {
            OnAbilityDeactivate();
            CooldownUIController.RemoveCooldown(_cooldownID);
            AbilityControlManager.Instance.UnregisterCallback(_abilityIndex);
        }

        protected void TryActivate()
        {
            if (_cooldownTimer > ModifiedCooldown)
            {
                Active = true;
                OnAbilityActivate();
                _activeTimer = 0;
                _cooldownTimer = 0;
            }
        }
        
        public override void OnUpdateOwner()
        {
            _cooldownTimer += Time.deltaTime;

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