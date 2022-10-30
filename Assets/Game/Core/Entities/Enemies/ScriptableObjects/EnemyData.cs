using System;
using TonyDev.Game.Core.Attacks;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.ScriptableObjects
{
    [Serializable]
    public class EnemyData
    {
        [Header("Basic Data")]
        public string enemyName;
        public int baseMoneyReward;
        public float hitboxRadius;

        [Header("Additional Data")]
        public EnemyAnimationPairs[] animations;

        [Header("Attack Data")]
        public AttackData contactAttackData;
    }

    [Serializable]
    public struct EnemyAnimationPairs
    {
        public EnemyAnimationState enemyAnimationState;
        public AnimationClip animation;
        public string AnimationName => animation.name;
    }
}
