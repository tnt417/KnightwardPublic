using System;
using TMPro;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TonyDev.Game.UI.GameInfo
{
    public class WaveUIController : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Slider waveSlider;
        [SerializeField] private Image waveFill;
        [SerializeField] private Image currentIcon;
        [SerializeField] private Image nextIcon;
        [FormerlySerializedAs("breakSprite")] [SerializeField] private Sprite bigWaveSprite;
        [SerializeField] private Sprite waveSprite;
        
        [SerializeField] private Sprite hotFill;
        [SerializeField] private Sprite coldFill;
        [SerializeField] private Sprite hostileFill;

        [SerializeField] private Animator fillAnimator;
        //

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "CastleScene") return; //Return if the not in the arena scene

            fillAnimator.speed = WaveManager.BreakPassingMultiplier;
            
            currentIcon.sprite = GameManager.Instance.wave % 8 == 0 ? bigWaveSprite : waveSprite;
            nextIcon.sprite = (GameManager.Instance.wave+1) % 8 == 0 ? bigWaveSprite : waveSprite;

            waveFill.sprite = GameManager.Instance.wave % 5 == 0 ? WaveManager.BreakPassingMultiplier > 0.5f ? hotFill : coldFill : hostileFill;
            
            waveSlider.value = GameManager.Instance.waveProgress;
        }
    }
}
