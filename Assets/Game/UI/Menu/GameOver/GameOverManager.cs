using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Menu.GameOver
{
    public class GameOverManager : MonoBehaviour
    {
        public void Replay()
        {
            GameManager.Reset();
            SceneManager.LoadScene("MainScene"); //When clicking the replay button, go back to the main scene.
        }
    }
}
