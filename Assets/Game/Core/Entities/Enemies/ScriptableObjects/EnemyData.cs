using System;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    [CreateAssetMenu(menuName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Data")]
        public string enemyName;
        public Sprite enemyBaseSprite;
        public int baseMoneyReward;
        public int maxHealth;
        public float hitboxRadius;
        
        [Header("Additional Data")]
        public RuntimeAnimatorController animatorController;
        public EnemyAnimationPairs[] animations;
        public EnemyMovementData movementData;
        public EnemyAttackData attackData;
    }

    [Serializable]
    public struct EnemyAnimationPairs
    {
        public EnemyAnimationState enemyAnimationState;
        public string animationName;
    }
}
