using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyScalingEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, (GameManager.EnemyDifficultyScale-1) * 0.05f/*0.1f * Mathf.Pow(GameManager.EnemyDifficultyScale/2, 1.3f)*/, EffectIdentifier);
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, (GameManager.EnemyDifficultyScale-1) * 0.10f/*0.2f * Mathf.Pow(GameManager.EnemyDifficultyScale/2, 1.4f)*/, EffectIdentifier);
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, (GameManager.EnemyDifficultyScale-1) * 0.02f/*0.05f * Mathf.Pow(GameManager.EnemyDifficultyScale/2, 1.3f)*/, EffectIdentifier);
            Entity.SetHealth(Entity.MaxHealth);

            var specialScore = Random.Range(0, 1f) + GameManager.EnemyDifficultyScale * 0.02f;

            if (specialScore > 0.9f)
            {
                // Entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Health, 1.5f, EffectIdentifier);
                // ChangeSize(2f);
            }else if (specialScore > 0.8f)
            {
                // Entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.Health, 0.8f, EffectIdentifier);
                // Entity.Stats.AddStatBonus(StatType.Multiplicative, Stat.MoveSpeed, 1.2f, EffectIdentifier);
                // ChangeSize(0.5f);
            }else if (false) //(specialScore > 0.78f)
            {
                Entity.AddEffect(new AttackEffect()
                {
                    ProjectileData = new ProjectileData()
                    {
                        prefabKey = "leafyPool",
                        projectileSprite = null,
                        attackData = new AttackData()
                        {
                            damageMultiplier = -0.1f,
                            hitboxRadius = 3,
                            knockbackMultiplier = 0,
                            team = Team.Player,
                            ignoreInvincibility = true
                        },
                        disableMovement = true,
                        doNotRotate = true,
                        childOfOwner = true
                    }
                }, Entity);
            }else if (specialScore > 0.76f)
            {
                Entity.AddEffect(new AttackEffect()
                {
                    ProjectileData = new ProjectileData()
                    {
                        prefabKey = "flameArmorPool",
                        projectileSprite = null,
                        attackData = new AttackData()
                        {
                            damageMultiplier = 0.1f,
                            hitboxRadius = 3,
                            knockbackMultiplier = 0,
                            team = Team.Enemy,
                            ignoreInvincibility = true
                        },
                        disableMovement = true,
                        doNotRotate = true,
                        childOfOwner = true
                    }
                }, Entity);
            }
        }

        private void ChangeSize(float multiplier)
        {
            foreach (var sr in Entity.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.transform.localScale *= multiplier;
            }
        }

        public override void OnRemoveOwner()
        {
            Entity.Stats.RemoveStatBonuses(EffectIdentifier);
        }

        public override void OnUpdateOwner()
        {
            
        }
    }
}
