using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private static Timer Instance { get; set; }
    [SerializeField] private TMP_Text gameTimerText;
    public static float GameTimer { get; private set; }
    private static bool paused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        paused = false;
    }

        // Update is called once per frame
    private void Update()
    {
        if(!paused) GameTimer += Time.deltaTime;
        gameTimerText.text = FormatTimeFromSeconds(GameTimer);
    }

    public static void Stop()
    {
        paused = true;
    }

    public static void Resume()
    {
        paused = false;
    }

    //TODO: Doesn't work for hours yet.
    private static string FormatTimeFromSeconds(float seconds)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append((int) (seconds / 60f)); // "XX:00"
        stringBuilder.Append(":");
        stringBuilder.Append(seconds % 60 < 10 ? "0" : null);
        stringBuilder.Append((int)(seconds % 60)); // "00:XX"
        
        return stringBuilder.ToString();
    }
}
