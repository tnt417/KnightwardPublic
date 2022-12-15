using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev
{
    public class StartScene : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
