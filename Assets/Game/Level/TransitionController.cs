using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Global;

namespace TonyDev.Game.Level
{
    [RequireComponent(typeof(Animator))]
    public class TransitionController : MonoBehaviour
    {
        public static TransitionController Instance;
        private Animator _transitionAnimator;
        private static string _queuedScene;
        [NonSerialized] public bool OutTransitionDone;

        public const float FadeOutTimeSeconds = 0.25f;

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        
            _transitionAnimator = GetComponent<Animator>(); //Initialize animator component

            if (GameManager.IsDemo)
            {
                _transitionAnimator.Play("SceneBlackout");
                return;
            }
            
            _transitionAnimator.Play("SceneFadeIn"); //Fade in when scene is loaded
        }

        public void FadeInOut() //Plays a fade out and fade in animation while transitioning to a new scene if valid
        {
            OutTransitionDone = false;
            _transitionAnimator.SetBool("ready", true);
            _transitionAnimator.Play("SceneFadeOut"); //Play the animation
        }

        public void BlackoutUntilFadeIn()
        {
            _transitionAnimator.Play("SceneBlackout");
        }

        public void FadeOut()
        {
            OutTransitionDone = false;
            _transitionAnimator.SetBool("ready", false);
            _transitionAnimator.Play("SceneFadeOut");
        }
        
        public void FadeIn()
        {
            _transitionAnimator.SetBool("ready", true);
        }

        public void FadeOutDone()
        {
            OutTransitionDone = true;
        }

        public static void TransitionScene(string destination)
        {
            TransitionSceneTask(destination).Forget();
        }

        private static async UniTask TransitionSceneTask(string destination)
        {
            Instance.FadeOut();
            await UniTask.WaitUntil(() => Instance.OutTransitionDone);
            await SceneManager.LoadSceneAsync(destination);
            Instance.FadeIn();
        }
    }
}
