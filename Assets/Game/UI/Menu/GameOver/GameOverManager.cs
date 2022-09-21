using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Menu.GameOver
{
    public class GameOverManager : MonoBehaviour
    {
        public void Replay()
        {
            SceneManager.LoadScene("MainMenuScene"); //When clicking the replay button, go back to the main scene.
        }
    }
}
