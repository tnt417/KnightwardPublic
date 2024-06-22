using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev
{
    public class PoisonClassEffect : GameEffect
    {
        public float BombCooldown = 3f;
        private float NextBombTime = 0f;

        private ProjectileData _bombData = new ProjectileData()
        {
            attackData = new AttackData()
            {
                damageMultiplier = 1f,
                destroyOnApply = false,
                destroyOnCollideWall = false,
                hitboxRadius = 0.25f,
                ignoreInvincibility = false,
                inflictEffects = new(),
                knockbackMultiplier = 0f,
                lifetime = 4f,
                spawnOnDestroyKey = "PoisonExplosion",
                team = Team.Player
            },
            childOfOwner = false,
            disableMovement = true,
            doNotRotate = true,
            effects = new List<GameEffect>(),
            offsetDegrees = 0f,
            prefabKey = "PoisonBomb",
            travelSpeed = 7f
        };

        public override void OnAddClient()
        {
            Entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Damage, 0.6f, "PoisonClass");
            Entity.OnDamageOther += OnDamageOther;
        }
        
        private void OnDamageOther(float dmg, GameEntity entity, bool crit, DamageType damageType)
        { 
            if (damageType == DamageType.DoT || entity == null) return;
            entity.CmdAddEffect(new PoisonEffect
            {
                Damage = dmg / 20, Frequency = 1f, Ticks = 20
            }, Entity);
        }

        public override void OnRemoveClient()
        {
            Entity.Stats.RemoveStatBonuses("PoisonClass");
        }

        public override void OnUpdateClient()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame && Time.time > NextBombTime)
            {
                NextBombTime = Time.time + BombCooldown;
                var originPos = (Vector2) Entity.transform.position;
                var direction = (GameManager.MousePosWorld - originPos).normalized;
                var proj = ObjectSpawner.SpawnProjectile(Entity, originPos, direction,
                    _bombData);
                var cont = proj.GetComponent<PoisonBombController>();
                cont.SetIntial(direction, 8f);
            }

            //TODO: Does this LINQ in update cause lag?

            var enemiesInRoom =
                GameManager.EntitiesReadonly.Where(ge => ge.IsAlive && ge.Team != Entity.Team && ge.CurrentParentIdentity == Entity.CurrentParentIdentity)
                    .ToList();

            if (enemiesInRoom.Count == 0) return;

            if (!enemiesInRoom.All(ge => PoisonEffect.GetPoisonDamage(ge) >= ge.NetworkCurrentHealth)) return;
            
            foreach (var ge in enemiesInRoom)
            {
                ge.CmdDamageEntity(99999, true, null, true, DamageType.Absolute);
            }
        }
    }
}