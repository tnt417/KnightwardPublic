using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance;
    private Animator _transitionAnimator;
    private static string _queuedScene;
    [NonSerialized] public bool InSceneTransition;
    [NonSerialized] public bool OutTransitionDone;

    private void Awake()
    {
        //Singleton code
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
        //
        
        _transitionAnimator = GetComponent<Animator>(); //Initialize animator component
        
        SceneManager.sceneLoaded += OnSceneLoaded; //Call OnSceneLoaded whenever a new scene is loaded.
        SceneManager.LoadScene("CastleScene"); //Load the CastleScene initially
    }

    public void FadeInOut() //Plays a fade out and fade in animation while transitioning to a new scene if valid
    {
        OutTransitionDone = false;
        _transitionAnimator.Play("SceneFadeOut"); //Play the animation
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _transitionAnimator.Play("SceneFadeIn"); //Fade in when a scene is loaded
    }

    public void LoadScene(string scene) //Transitions to a new scene
    {
        _queuedScene = scene; //Update queuedScene variable.
        InSceneTransition = true; //We are now in scene transition
        FadeInOut(); //Transition to new scene
    }

    public void GoToQueuedScene() //Tries to go to the queued scene, if it is valid.
    {
        OutTransitionDone = true; //Mark the transition as finished
        if (_queuedScene == null || _queuedScene == SceneManager.GetActiveScene().name) return; //Return if not a valid scene destination
        InSceneTransition = false;
        SceneManager.LoadScene(_queuedScene); //Loads the queued scene
    }
}
