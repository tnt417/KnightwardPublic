using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Level
{
    [RequireComponent(typeof(Animator))]
    public class TransitionController : MonoBehaviour
    {
        public static TransitionController Instance;
        private Animator _transitionAnimator;
        private static string _queuedScene;
        [NonSerialized] public bool OutTransitionDone;

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        
            _transitionAnimator = GetComponent<Animator>(); //Initialize animator component
            
            _transitionAnimator.Play("SceneFadeIn"); //Fade in when scene is loaded
        }

        public void FadeInOut() //Plays a fade out and fade in animation while transitioning to a new scene if valid
        {
            OutTransitionDone = false;
            _transitionAnimator.Play("SceneFadeOut"); //Play the animation
        }

        public void FadeOutDone()
        {
            OutTransitionDone = true;
        }
    }
}
