using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TonyDev
{
    public class PauseController : MonoBehaviour
    {
        public GameObject uiObject;
        public GameObject settingsObject;

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

        public void OnSettings()
        {
            uiObject.SetActive(false);
            settingsObject.SetActive(true);
        }
        
        public void OnOpen()
        {
            uiObject.SetActive(true);
            settingsObject.SetActive(false);
            if(NetworkServer.connections.Count == 1) Time.timeScale = 0;
        }
        
        public void OnClose()
        {
            uiObject.SetActive(false);
            settingsObject.SetActive(false);
            Time.timeScale = 1;
        }
        
        public void OnQuit()
        {
            Time.timeScale = 1;
            NetworkManager.singleton.StopHost();
            Application.Quit(0);
        }
    }
}
