using System;
using Cysharp.Threading.Tasks;
using TMPro;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Menu.GameOver
{
    public static class PlayStats
    {
        public static bool GameWon = false;
        public static float GameTimeSeconds = 0;
        public static int FloorsCompleted = 0;
    }

    public class GameOverManager : MonoBehaviour
    {
        public TMP_Text WinLoseText;
        public TMP_Text ProgressText;

        private void Awake()
        {
            WinLoseText.text = PlayStats.GameWon ? "Victory!" : "Better luck next time...";
            ProgressText.text = $"You completed {PlayStats.FloorsCompleted} floors in {Timer.FormatTimeFromSeconds(PlayStats.GameTimeSeconds)}!";
        }

        private async UniTask TransitionMenu()
        {
            TransitionController.Instance.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
            SceneManager.LoadScene("MainMenuScene");
            TransitionController.Instance.FadeIn();
        }
        
        public void Replay()
        {
            TransitionMenu().Forget();
        }
    }
}
