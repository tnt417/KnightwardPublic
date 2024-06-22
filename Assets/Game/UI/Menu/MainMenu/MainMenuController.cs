using System;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Menu.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float menuShiftEffectStrength = 1f; //The strength of the moving camera effect
        private Vector3 _originalCameraPos;

        private void Start()
        {
            _originalCameraPos = mainCamera.transform.position;
            
            GameManager.ResetGame();

            Debug.unityLogger.logEnabled = PlayerPrefs.GetInt("errorLog", 1) == 1;
        }

        private void Update()
        {
            mainCamera.transform.position =
                _originalCameraPos + (Vector3)(Mouse.current.position.ReadValue() - new Vector2(Screen.width / 2f, Screen.height / 2f)) * menuShiftEffectStrength/1000f; //Offset the camera pos based on mouse pos.
        }

        private bool _hostClicked = false;
        
        public void OnHostClick()
        {
            if (_hostClicked) return;
            _hostClicked = true;
            
            var customNetManager = NetworkManager.singleton as CustomNetworkManager;
            if (customNetManager != null)
            {
                TransitionHost(customNetManager).Forget();
            }
        }

        private async UniTask TransitionHost(CustomNetworkManager manager)
        {
            TransitionController.Instance.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
            await manager.CreateAndHost();
            TransitionController.Instance.FadeIn();
        }
        
        public void OnJoinClick()
        {
            SteamLobbyManager.Singleton.ActivateJoinOverlay();
        }
        
        public void QuitGame()
        {
            Application.Quit(0);
        }
    }
}
