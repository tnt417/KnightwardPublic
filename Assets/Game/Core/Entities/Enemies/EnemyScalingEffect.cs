using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyScalingEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, 0.05f * Mathf.Pow(GameManager.EnemyDifficultyScale*2, 1.3f), "EnemyScaling");
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Health, 0.05f * Mathf.Pow(GameManager.EnemyDifficultyScale*2, 1.6f), "EnemyScaling");
        }

        public override void OnRemoveOwner()
        {
            Entity.Stats.RemoveStatBonuses("EnemyScaling");
        }

        public override void OnUpdateOwner()
        {
            
        }
    }
}
