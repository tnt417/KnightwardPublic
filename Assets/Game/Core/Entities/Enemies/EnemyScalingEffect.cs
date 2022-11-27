using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyScalingEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, 0.07f * Mathf.Pow(GameManager.EnemyDifficultyScale*2, 1.15f), EffectIdentifier);
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, 0.1f * Mathf.Pow(GameManager.EnemyDifficultyScale*2, 1.3f), EffectIdentifier);
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
