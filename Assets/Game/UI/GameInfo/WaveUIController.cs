using System;
using TMPro;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private Sprite breakSprite;
        [SerializeField] private Sprite waveSprite;
        
        [SerializeField] private Sprite hotFill;
        [SerializeField] private Sprite coldFill;
        [SerializeField] private Sprite hostileFill;

        [SerializeField] private Animator fillAnimator;
        //

        private WaveManager _waveManager;

        private void Start()
        {
            _waveManager = FindObjectOfType<WaveManager>();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "CastleScene") return; //Return if the not in the arena scene

            fillAnimator.speed = WaveManager.BreakPassingMultiplier;
            
            currentIcon.sprite = _waveManager.wavesSpawned % 5 == 0 ? breakSprite : waveSprite;
            nextIcon.sprite = (_waveManager.wavesSpawned+1) % 5 == 0 ? breakSprite : waveSprite;

            waveFill.sprite = _waveManager.wavesSpawned % 5 == 0 ? WaveManager.BreakPassingMultiplier > 0.5f ? hotFill : coldFill : hostileFill;
            
            waveSlider.value = GameManager.Instance.waveProgress;
        }
    }
}
