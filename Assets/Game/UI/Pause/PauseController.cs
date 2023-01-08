using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TonyDev
{
    public class PauseController : MonoBehaviour
    {
        public GameObject uiObject;

        public Button quitButton;
        
        public static bool Paused { get; private set; }

        private void Awake()
        {
            Paused = false;
            OnClose();
            quitButton.onClick.AddListener(OnQuit);
        }

        public void OnPause(InputValue value)
        {
            if (!value.isPressed) return;
            Paused = !Paused;
            
            if (Paused)
            {
                OnOpen();
            }
            else
            {
                OnClose();
            }
        }
        
        public void OnOpen()
        {
            uiObject.SetActive(true);
            if(NetworkServer.connections.Count == 1) Time.timeScale = 0;
        }
        
        public void OnClose()
        {
            uiObject.SetActive(false);
            Time.timeScale = 1;
        }
        
        public void OnQuit()
        {
            Time.timeScale = 1;
            NetworkManager.singleton.StopHost();
        }
    }
}
