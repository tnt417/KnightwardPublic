using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAnimationState
{
    Hurt,
    Move,
    Stop,
    Attack
}

public class EnemyAnimator : MonoBehaviour
{
    //Editor Variables
    [SerializeField] private string hurtAnimationName;
    [SerializeField] private EnemyMoveBase enemyMoveBase;
    [SerializeField] private Animator animator;
    //

    public void PlayAnimation(EnemyAnimationState state)
    {
        switch (state)
        {
            case EnemyAnimationState.Hurt:
                animator.Play(hurtAnimationName);
            break;
            case EnemyAnimationState.Move:
                animator.Play(enemyMoveBase.animations[0].name);
                break;
            case EnemyAnimationState.Stop:
                animator.Play(enemyMoveBase.animations[1].name);
                break;
            case EnemyAnimationState.Attack:
                animator.Play(enemyMoveBase.animations[2].name);
                break;
        }
    }
}