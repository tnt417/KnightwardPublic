using TonyDev.Game.Core.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.UI.Healthbar
{
    public class Healthbar : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Slider healthSlider;
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

            _attachedDamageable.OnHealthChanged += UpdateUI; //Set the UI to be updated whenever the health is changed
        }

        private void UpdateUI()
        {
            healthSlider.maxValue = _attachedDamageable.MaxHealth; //Update the slider values.
            healthSlider.value = _attachedDamageable.CurrentHealth;
        }
    }
}
