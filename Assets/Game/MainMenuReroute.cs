using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev
{
    public class MainMenuReroute : MonoBehaviour
    {
        private void Awake()
        {
            if (!NetworkServer.active)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}
