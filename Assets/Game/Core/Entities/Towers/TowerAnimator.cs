using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public enum TowerAnimationState
    {
        Charge, Fire
    }
    public class TowerAnimator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip[] animations; //Animations should correspond to the TowerAnimationState. Ex. animations[0] corresponds to Charge
        //
        
        public void PlayAnimation(TowerAnimationState animState)
        {
            animator.Play(animations[(int)animState].name); //Play an animation based on the animState
        }
    }
}
