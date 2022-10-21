using System;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Enemies.Movement;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Prefab - can be left empty for default")]
        public GameObject prefab;
        
        [Header("Basic Data")]
        public string enemyName;
        public int baseMoneyReward;
        public float hitboxRadius;

        [Header("Stats - some stats may not be implemented")] 
        public StatBonus[] baseStats;

        [Header("Additional Data")] 
        public RuntimeAnimatorController animatorController;
        public EnemyAnimationPairs[] animations;
        public BehaviorTimelineData timelineData;
        
        [Header("Attack Data")]
        public AttackData contactAttackData;
        public ProjectileData[] projectileAttackData;
    }

    [Serializable]
    public struct EnemyAnimationPairs
    {
        public EnemyAnimationState enemyAnimationState;
        public string animationName;
    }
}
