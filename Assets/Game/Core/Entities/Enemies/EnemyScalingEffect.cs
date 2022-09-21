using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class EnemyScalingEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Entity.Stats.AddStatBonus(StatType.AdditivePercent, Stat.Damage, 0.05f * GameManager.EnemyDifficultyScale, "EnemyScaling");
            Entity.Stats.AddStatBonus(StatType.Flat, Stat.Armor, 5f * GameManager.EnemyDifficultyScale, "EnemyScaling");
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
