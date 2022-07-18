using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public enum TowerAnimationState
    {
        Charge, Fire, Idle
    }
    public class TowerAnimator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip[] animations; //Animations should correspond to the TowerAnimationState. Ex. animations[0] corresponds to Charge
        //
        
        public void PlayAnimation(TowerAnimationState animState)
        {
            if (animations.Length < (int)animState || animations[(int) animState] == null) return;
            animator.Play(animations[(int)animState].name); //Play an animation based on the animState
        }
    }
}
