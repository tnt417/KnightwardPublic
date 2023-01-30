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
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.MoveSpeed, (GameManager.EnemyDifficultyScale-1) * 0.04f/*0.05f * Mathf.Pow(GameManager.EnemyDifficultyScale/2, 1.3f)*/, EffectIdentifier);
            Entity.SetHealth(Entity.MaxHealth);
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
