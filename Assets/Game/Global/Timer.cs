using System.Text;
using Mirror;
using TMPro;
using TonyDev.Game.Global.Console;
using UnityEngine;

namespace TonyDev.Game.Global
{
    public class Timer : MonoBehaviour
    {
        private static Timer Instance { get; set; }
        [SerializeField] private TMP_Text gameTimerText;
        public static float GameTimer;
        public static float TickSpeedMultiplier = 1f;
        private static bool _paused = false;

        private void Awake()
        {
            //Singleton code
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            //
        }
    
        private void Update()
        {
            if(!_paused) GameTimer += Time.deltaTime; //Tick timer if not paused
            gameTimerText.text = FormatTimeFromSeconds(GameTimer); //Update timer text
        }

        [GameCommand(Keyword = "time", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Set time.")] [ServerCallback]
        public static void SetTimeSeconds(int time)
        {
            GameTimer = time;
        }

        public static void Stop()
        {
            _paused = true; //Pause the timer
        }

        public static void Resume()
        {
            _paused = false; //Resume the timer
        }

        //TODO: Doesn't work for hours yet.
        public static string FormatTimeFromSeconds(float seconds) //Turns seconds into a readable string in the format MM:SS
        {
            var stringBuilder = new StringBuilder();
        
            stringBuilder.Append((int) (seconds / 60f)); // "XX:00"
            stringBuilder.Append(":");
            stringBuilder.Append(seconds % 60 < 10 ? "0" : null);
            stringBuilder.Append((int)(seconds % 60)); // "00:XX"
        
            return stringBuilder.ToString();
        }
    }
}
