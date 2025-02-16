using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class SettingsController : MonoBehaviour
    {
        public Slider volSlider;
        public TMP_Text volLabel;

        public Slider shakeSlider;
        public TMP_Text shakeLabel;

        public Toggle errorLogToggle;
        public Toggle fsToggle;
        
        private void Awake()
        {
            volSlider.value = SoundManager.MasterVolume;
            
            volLabel.text = "VOL\n" + $"{volSlider.normalizedValue*2:P0}";

            volSlider.onValueChanged.AddListener((newValue) =>
            {
                SoundManager.SetMasterVolume(newValue);
                volLabel.text = "VOL\n" + $"{volSlider.normalizedValue*2:P0}";
            });

            var fs = PlayerPrefs.GetInt("Fullscreen", 1);

            if (fs == 1)
            {
                Screen.fullScreen = true;
            }
            
            fsToggle.isOn = fs == 1;
            
            shakeSlider.value = SmoothCameraFollow.ShakeMultiplier;
            
            shakeLabel.text = "SHAKE\n" + $"{shakeSlider.normalizedValue*2:P0}";

            shakeSlider.onValueChanged.AddListener((newValue) =>
            {
                SmoothCameraFollow.SetShakeMultiplier(newValue);
                shakeLabel.text = "SHAKE\n" + $"{shakeSlider.normalizedValue*2:P0}";
            });

            errorLogToggle.interactable = Debug.isDebugBuild;

            errorLogToggle.isOn = Debug.isDebugBuild && Debug.unityLogger.logEnabled;
            
            errorLogToggle.onValueChanged.AddListener((newVal) =>
            {
                PlayerPrefs.SetInt("errorLog", newVal ? 1 : 0);
                Debug.unityLogger.logEnabled = newVal;
            });
            
            fsToggle.onValueChanged.AddListener((newVal) =>
            {
                PlayerPrefs.SetInt("Fullscreen", newVal ? 1 : 0);
                Screen.fullScreen = newVal;
            });
        }
    }
}
