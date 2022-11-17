using System;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Weapon.Broadsword
{
    public class BroadswordEffect : AbilityEffect
    {
        public float RefundCooldownAmount;
        public float BonusExpirationTime;
        public float AttackSpeedBonusMultiplier;
        public float MoveSpeedBonusMultiplier;
        public int MaxSuccession;

        public override void OnAddOwner()
        {
            base.OnAddOwner();
            Entity.OnTryHurtInvulnerableOwner += TryDeflect;
            Entity.OnHurtOwner += ResetDeflects;

            _broadswordParticleEffect = new ParticleTrailEffect
            {
                OverridePrefab = "broadswordParticles",
            };

            Entity.OnAttack += PlaySound;
            
            //GameEffect.RegisterEffect(_broadswordParticleEffect);
            
            Entity.CmdAddEffect(_broadswordParticleEffect, Entity);
        }

        private void PlaySound()
        {
            SoundManager.PlaySoundPitchVariant("dagger", Entity.transform.position, 0.6f, 0.8f);
        }

        [NonSerialized] private int _successiveDeflects = 0;
        [NonSerialized] private double _deflectExpirationTime;

        private ParticleTrailEffect _broadswordParticleEffect;

        private void TryDeflect(float damage)
        {
            var pos = Entity.transform.position;
            
            ObjectSpawner.SpawnTextPopup(pos, "Deflected!", Color.blue, 0.5f);
            DiscountCooldown(RefundCooldownAmount, true); //Refund cooldown upon blocking

            SoundManager.PlaySound("anvil", pos);
            
            _successiveDeflects++;

            _broadswordParticleEffect.SetVisible(true);

            var main = _broadswordParticleEffect.ParticleSystem.main;
            main.simulationSpeed = 1 + _successiveDeflects * 0.2f;

            var emission = _broadswordParticleEffect.ParticleSystem.emission;
            emission.rateOverTime = 15 * _successiveDeflects;

            _successiveDeflects = Math.Clamp(_successiveDeflects, 0, MaxSuccession);
            _deflectExpirationTime = Time.time + BonusExpirationTime;

            Entity.Stats.RemoveBuffsOfSource("BroadswordEffect");
            Entity.Stats.AddBuff(
                new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed,
                    AttackSpeedBonusMultiplier * _successiveDeflects, "BroadswordEffect"), BonusExpirationTime);
            Entity.Stats.AddBuff(
                new StatBonus(StatType.AdditivePercent, Stat.MoveSpeed, MoveSpeedBonusMultiplier * _successiveDeflects,
                    "BroadswordEffect"), BonusExpirationTime);

            Entity.IsInvulnerable = false;
        }

        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();
            if (Time.time > _deflectExpirationTime)
            {
                ResetDeflects(0);
            }
        }

        private void ResetDeflects(float damage)
        {
            Entity.Stats.RemoveBuffsOfSource("BroadswordEffect");
            _broadswordParticleEffect.SetVisible(false);
            _successiveDeflects = 0;
        }

        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            Entity.OnTryHurtInvulnerableOwner -= TryDeflect;
            Entity.OnHurtOwner -= ResetDeflects;
            Entity.OnAttack -= PlaySound;
            Entity.CmdRemoveEffect(_broadswordParticleEffect);
        }

        protected override void OnAbilityActivate()
        {
            Entity.IsInvulnerable = true;
        }

        protected override void OnAbilityDeactivate()
        {
            Entity.IsInvulnerable = false;
        }
    }
}