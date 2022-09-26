using System;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Effects
{
    public class ElementalGloveEffect : AbilityEffect
    {
        private enum Effect
        {
            Slow, Burn, Leech
        }
        
        public float SlowAmount;
        public float BurnDamagePerSecondMultiplier;
        public float Frequency;
        public int Ticks;
        public float LeechPercent;

        private Effect _activeEffect;

        private ParticleTrailEffect _trailEffect;

        public override void OnAddOwner()
        {
            UpdateCooldown(PlayerInventory.Instance.WeaponItem);
            PlayerInventory.OnItemInsertLocal += UpdateCooldown;
            
            _trailEffect = new ParticleTrailEffect();
            Entity.CmdAddEffect(_trailEffect, Entity);
            _trailEffect.SetVisible(false);
            
            Entity.OnDamageOther += TryLeech;
            
            base.OnAddOwner();
        }

        private void UpdateCooldown(Item item)
        {
            if (item.itemType == ItemType.Weapon)
            {
                Cooldown = item.itemEffects.Where(ie => ie is AbilityEffect)
                    .Select(ge => ((AbilityEffect) ge).Cooldown).FirstOrDefault();
                Duration = Cooldown;
            }
        }

        public override void OnRemoveOwner()
        {
            Entity.CmdRemoveEffect(_trailEffect);
            PlayerInventory.OnItemInsertLocal -= UpdateCooldown;
            base.OnRemoveOwner();
        }

        private GameEffect _speedEffect;
        private GameEffect _burnEffect;
        
        protected override void OnAbilityActivate()
        {
            _activeEffect = (Effect) Random.Range(0, 3);
            _trailEffect.SetVisible(true);
            switch (_activeEffect)
            {
                case Effect.Burn:
                    _trailEffect.SetColor(Color.red);
                    _burnEffect = new PoisonEffect
                    {
                        Damage = BurnDamagePerSecondMultiplier * Entity.DamageMultiplier * Frequency,
                        Frequency = Frequency,
                        Ticks = Ticks
                    };
                    PlayerInventory.Instance.WeaponItem.AddInflictEffect(_burnEffect, false);
                    break;
                case Effect.Slow:
                    _trailEffect.SetColor(Color.cyan);
                    _speedEffect = new SpeedEffect
                    {
                        duration = Duration,
                        moveSpeedMultiplier = 1 - SlowAmount,
                        Source = Entity
                    };
                    PlayerInventory.Instance.WeaponItem.AddInflictEffect(_speedEffect, false);
                    break;
                case Effect.Leech:
                    _trailEffect.SetColor(Color.green);
                    Entity.OnDamageOther += TryLeech;
                    break;
            }
        }

        protected void TryLeech(float dmg, GameEntity other, bool isCrit)
        {
            if (_activeEffect == Effect.Leech)
            {
                var leech = -dmg * LeechPercent;
                
                ObjectSpawner.SpawnDmgPopup(Entity.transform.position, (int)leech, isCrit);
                
                Entity.ApplyDamage(leech);
            }
        }
        
        protected override void OnAbilityDeactivate()
        {
            _trailEffect.SetVisible(false);
            switch (_activeEffect)
            {
                case Effect.Burn:
                    PlayerInventory.Instance.WeaponItem.RemoveInflictEffect(_burnEffect);
                    break;
                case Effect.Slow:
                    PlayerInventory.Instance.WeaponItem.RemoveInflictEffect(_speedEffect);
                    break;
                case Effect.Leech:
                    Entity.OnDamageOther -= TryLeech;
                    break;
            }
        }
    }
}