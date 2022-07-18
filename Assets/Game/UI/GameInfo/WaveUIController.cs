using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev
{
    public class WaveUIController : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text waveText;
        //
        
        private WaveManager _waveManager;

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "CastleScene") return; //Return if the not in the arena scene
            
            if(_waveManager == null) _waveManager = FindObjectOfType<WaveManager>(); //Initialize the WaveManager if it isn't initialized
            
            waveText.text = _waveManager.wavesSpawned.ToString(); //Update wave number text.
            timerText.text = _waveManager.TimeUntilNextWaveSeconds.ToString(); //Update timer text.
        }
    }
}
