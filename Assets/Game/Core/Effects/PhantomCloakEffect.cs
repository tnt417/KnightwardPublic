using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PhantomCloakEffect : GameEffect
    {
        public float StandStillTimer;
        public float MoveSpeedBonus;
        public float AttackBonus;
        
        public override void OnAddOwner()
        {
            Entity.OnDamageOther += (f, entity, arg3, arg4) =>
            {
                if(f > 0) Deactivate();
            };
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= (f, entity, arg3, arg4) =>
            {
                if(f > 0) Deactivate();
            };
            
            Deactivate();
        }

        private Vector2 _lastPlayerPos;
        private float _standTimer;
        private InvisibilityEffect _appliedEffect;

        public override void OnUpdateOwner()
        {
            if (_appliedEffect != null) return;

            if (Vector2.Distance(_lastPlayerPos, (Vector2) Entity.transform.position) > 0.001f)
            {
                OnMove();
            }
            
            _lastPlayerPos = Entity.transform.position;

            _standTimer += Time.deltaTime;

            Debug.Log(_standTimer);
            
            if (_standTimer > StandStillTimer)
            {
                _standTimer = 0;
                Trigger();
            }
        }

        private void Trigger()
        {
            _appliedEffect = new InvisibilityEffect();
            Entity.CmdAddEffect(_appliedEffect, Source);
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, MoveSpeedBonus, "PhantomCloak");
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, AttackBonus, "PhantomCloak");
        }
        
        private void OnMove()
        {
            Debug.Log("Move!");
            
            if (_appliedEffect == null)
            {
                _standTimer = 0;
            }
        }

        private void Deactivate()
        {
            if (_appliedEffect != null)
            {
                Entity.CmdRemoveEffect(_appliedEffect);
                Entity.Stats.RemoveStatBonuses("PhantomCloak");
                _appliedEffect = null;
            }
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>After standing still for <color=yellow>{StandStillTimer:N0}</color> seconds, turn invisible and gain move speed and damage until you attack something.</color>";
        }
    }
}