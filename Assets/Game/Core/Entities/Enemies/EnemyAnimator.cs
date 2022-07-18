using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Enemies.Movement;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public enum EnemyAnimationState
    {
        Hurt,
        Move,
        Stop,
        Attack,
        Die
    }

    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        //Editor Variables
        [SerializeField] private Animator animator;
        //
        
        private readonly Dictionary<EnemyAnimationState, string> _animationPairs = new();

        public void PlayAnimation(EnemyAnimationState state)
        {
            if (!_animationPairs.ContainsKey(state)) return;
            animator.Play(_animationPairs[state]);
        }

        public void Set(EnemyData enemyData)
        {
            animator.runtimeAnimatorController = enemyData.animatorController;
            
            IEnumerable<EnemyAnimationPairs> animationPairs = enemyData.animations;
            foreach (var ap in animationPairs)
            {
                _animationPairs.Add(ap.enemyAnimationState, ap.animationName);
            }
        }
    }
}