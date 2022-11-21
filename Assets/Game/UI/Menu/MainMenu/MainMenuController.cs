using UnityEngine;
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
        }

        private void Update()
        {
            mainCamera.transform.position =
                _originalCameraPos + (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f)) * menuShiftEffectStrength/1000f; //Offset the camera pos based on mouse pos.
        }

        public void OnPlayClick()
        {
            SceneManager.LoadScene("LobbyScene"); //Go to main scene when the play button is clicked
        }

        public void QuitGame()
        {
            Application.Quit(0);
        }
    }
}
