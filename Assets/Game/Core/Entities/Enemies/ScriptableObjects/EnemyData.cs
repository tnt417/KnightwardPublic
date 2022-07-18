using System;
using TonyDev.Game.Core.Entities.Enemies.Attack;
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
