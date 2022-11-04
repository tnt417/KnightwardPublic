using System;
using TonyDev.Game.Core.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Healthbar
{
    public class Healthbar : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        //
    
        private IDamageable _attachedDamageable; //The damageable component this health bar is dedicated to
    
        private void Start()
        {
            _attachedDamageable = GetComponentInParent<IDamageable>(); //Initialize the IDamageable component
        
        
            //Destroy the health bar if there is no IDamageable component attached and print a warning.
            if (_attachedDamageable == null)
            {
                Debug.LogWarning("No attached IDamageable component! Removing healthbar...");
                Destroy(gameObject);
                return;
            }
            //

            //Initialize slider values
            healthSlider.maxValue = _attachedDamageable.MaxHealth;
            healthSlider.value = _attachedDamageable.CurrentHealth;

            _attachedDamageable.OnHealthChangedOwner += (float value) => UpdateUI(); //Set the UI to be updated whenever the health is changed
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_attachedDamageable is GameEntity gameEntity)
            {
                healthSlider.maxValue = gameEntity.NetworkMaxHealth; //Update the slider values.
                healthSlider.value = gameEntity.NetworkCurrentHealth + gameEntity.ClientHealthDisparity;
                fillImage.color = gameEntity.IsInvulnerable
                    ? Color.blue//new Color(0.572549f, 0.909804f, 0.7529413f)
                    : new Color(0.3882353f, 0.6705883f, 0.2470588f);
            }
            else
            {
                healthSlider.maxValue = _attachedDamageable.MaxHealth; //Update the slider values.
                healthSlider.value = _attachedDamageable.CurrentHealth;
            }
        }
    }
}
