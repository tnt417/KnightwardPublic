using System;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Menu.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        private Vector3 _originalShiftPos;

        private void Start()
        {
            GameManager.ResetGame();

            Debug.unityLogger.logEnabled = PlayerPrefs.GetInt("errorLog", 1) == 1;
            
            if (/*GameManager.IsDemo ||*/ GameManager.QuickTestMode)
            {
                var customNetManager = NetworkManager.singleton as CustomNetworkManager;
            
                if (customNetManager != null)
                {
                    customNetManager.CreateAndHost().Forget();
                }
            }
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
            if (GameManager.IsDemo)
            {
                TransitionController.Instance.BlackoutUntilFadeIn();
                return;
            }
            TransitionController.Instance.FadeIn();
        }
        
        public void OnJoinClick()
        {
            SteamLobbyManager.Singleton.ActivateJoinOverlay();
            
            var cnm = NetworkManager.singleton as CustomNetworkManager;
            if (cnm != null && cnm.TelepathyServer) cnm.ConnectToAddress("localhost");
        }
        
        public void QuitGame()
        {
            Application.Quit(0);
        }

        public void OnUpgradesClick()
        {
            TransitionUpgrades().Forget();
        }
        
        private async UniTask TransitionUpgrades()
        {
            TransitionController.Instance.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
            await SceneManager.LoadSceneAsync("UpgradeScene");
            TransitionController.Instance.FadeIn();
        }
    }
}
