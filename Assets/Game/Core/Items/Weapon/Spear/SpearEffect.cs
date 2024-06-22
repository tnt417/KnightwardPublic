using System;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [Serializable]
    public class SpearEffect : AbilityEffect
    {
        public float MoveSpeedMultiplier;
        public float AttackSpeedMultiplier;
        public float AoeMultiplier;
        public float PoisonDPSMultiplier;

        public ProjectileData regularProjectile;
        public ProjectileData empoweredProjectile;

        private bool _empowered;

        public override void OnAddOwner()
        {
            ActivateButton = KeyCode.None;
            base.OnAddOwner();
            empoweredProjectile.OnHitOther += InflictPoison;
            Entity.OnAttack += Shoot;
            Player.LocalInstance.playerAnimator.attackAnimationName = "Throw";
        }

        public void Shoot()
        {
            var spawnPos = (Vector2)Entity.transform.position - new Vector2(0, 0.4f);
            var direction = GameManager.MouseDirectionLow;

            SoundManager.PlaySoundPitchVariant("spear", 0.5f,spawnPos, 0.8f, 1.2f);
            SmoothCameraFollow.Shake(1, 3f);
            
            if (_empowered)
            {
                //var direction1 = GameTools.Rotate(direction, -10 * Mathf.Deg2Rad);
                //var direction2 = GameTools.Rotate(direction, 10 * Mathf.Deg2Rad);
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction, empoweredProjectile);
                //ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction1, empoweredProjectile);
                //ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction2, empoweredProjectile);
            }
            else
            {
                ObjectSpawner.SpawnProjectile(Entity, spawnPos, direction, regularProjectile);
            }
        }

        public override void OnUpdateOwner()
        {
            base.OnUpdateOwner();
            
            if (Mouse.current.rightButton.isPressed && Ready)
            {
                OnAbilityActivate();
            }
        }

        public override void OnRemoveOwner()
        {
            empoweredProjectile.OnHitOther -= InflictPoison;
            Entity.OnAttack -= Shoot;
            base.OnRemoveOwner();
        }

        public void InflictPoison(float dmg, GameEntity entity, bool crit, DamageType dt)
        {
            if (entity == null) return;
            
            entity.CmdAddEffect(new PoisonEffect()
            {
                Damage = PoisonDPSMultiplier * Entity.Stats.GetStat(Stat.Damage),
                Ticks = 10,
                Frequency = 1f,
            }, Entity);
        }

        protected override void OnAbilityActivate()
        {
            base.OnAbilityActivate();
            OnAbilityDeactivate();
            
            _empowered = true;

            Entity.Stats.AddBuff(
                new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, MoveSpeedMultiplier, "spearEffect"), Duration);
            Entity.Stats.AddBuff(
                new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, AttackSpeedMultiplier, "spearEffect"),
                Duration);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.AoeSize, AoeMultiplier, "spearEffect"),
                Duration);
        }

        protected override void OnAbilityDeactivate()
        {
            Entity.Stats.RemoveBuffsOfSource("spearEffect");
            _empowered = false;
        }
    }
}