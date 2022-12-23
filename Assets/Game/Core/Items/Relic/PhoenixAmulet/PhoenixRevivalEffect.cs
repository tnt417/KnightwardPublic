using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class PhoenixRevivalEffect : AbilityEffect
    {
        public float RevivalHealthPercentage = 0.25f; // The percentage of maximum health the player is revived with
        public float InvincibilityDuration = 3.0f; // The duration of invincibility after revival
        public float RevivalRange = 5.0f; // The radius of the revival explosion

        private Player _player;
        
        // Called on the server when the effect is added to a GameEntity
        public override void OnAddOwner()
        {
            base.OnAddOwner();
            DiscountCooldown(1, true);
            
            if (Entity is not Player entity)
            {
                Entity.CmdRemoveEffect(this);
                return;
            }
            
            _player = entity;
            entity.OnDeathOwner += OnDeath;
        }

        // Called on the server when the effect is removed from a GameEntity
        public override void OnRemoveOwner()
        {
            base.OnRemoveOwner();
            Entity.OnDeathOwner -= OnDeath;
        }

        protected override void OnAbilityActivate()
        {
        }

        public override void OnUpdateOwner()
        {
            _player.playerDeath.disableDeathHandling = Ready;
            base.OnUpdateOwner();
        }

        // Called on the server when the entity dies
        private void OnDeath(float a)
        {
            if(!Ready) return;

            // Calculate the amount of health to revive the player with
            var revivalHealth = Entity.MaxHealth * RevivalHealthPercentage;

            // Revive the player

            Entity.SetHealth(revivalHealth);

            base.OnAbilityActivate();

            Object.Instantiate(ObjectFinder.GetPrefab("phoenix"), Entity.transform.position, Quaternion.identity);
            
            // Deal damage to enemies within the revival range
            foreach (var enemy in GameManager.GetEntitiesInRange(Entity.transform.position, RevivalRange))
            {
                if (enemy.Team != Entity.Team)
                {
                    var crit = Entity.Stats.CritSuccessful;
                    enemy.CmdDamageEntity(crit ? revivalHealth * 2 : revivalHealth, crit, null, false);
                }
            }
            
            // Apply the invincibility effect to the player
            var invincibilityEffect = new InvincibilityEffect
            {
                Duration = InvincibilityDuration
            };
            Entity.CmdAddEffect(invincibilityEffect, Entity);
        }

        public override string GetEffectDescription()
        {
            return
                $"<color=green>Revives the player with <color=yellow>{RevivalHealthPercentage:P0}</color> of their maximum health, deals <color=yellow>{RevivalHealthPercentage:P0}</color> of the player's maximum health as damage to enemies within a <color=yellow>{RevivalRange}</color> unit radius, and grants <color=yellow>{InvincibilityDuration:F1}</color> seconds of invincibility upon revival.</color>";
        }
    }
    
    public class InvincibilityEffect : GameEffect
    {
        public float Duration; // The duration of the invincibility effect

        private float _elapsedTime; // The elapsed time since the effect was applied

        // Called on the server every frame while the effect is applied to a GameEntity
        public override void OnUpdateClient()
        {
            Entity.IsInvulnerable = true;
            
            // Increment the elapsed time
            _elapsedTime += Time.deltaTime;

            // Check if the duration has been reached
            if (_elapsedTime >= Duration && Entity.EntityOwnership)
            {
                // Remove the effect
                Entity.CmdRemoveEffect(this);
            }
        }

        // Called on the server when the effect is added to a GameEntity
        public override void OnAddClient()
        {
            // Set the entity's invincibility flag to true
            Entity.IsInvulnerable = true;

            // Reset the elapsed time
            _elapsedTime = 0;
        }

        // Called on the server when the effect is removed from a GameEntity
        public override void OnRemoveClient()
        {
            // Set the entity's invincibility flag to false
            Entity.IsInvulnerable = false;
        }
    }
    
}