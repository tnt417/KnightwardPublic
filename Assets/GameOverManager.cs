using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void Replay()
    {
        SceneManager.LoadScene("MainScene"); //When clicking the replay button, go back to the main scene.
    }
}
