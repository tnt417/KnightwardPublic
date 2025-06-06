using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Healthbar
{
    public class Healthbar : MonoBehaviour
    {
        //Editor variables
        [SerializeField] protected Slider healthSlider;
        [SerializeField] protected Slider lagBehindSlider;
        [SerializeField] protected Slider shieldSlider;
        [SerializeField] protected Slider decaySlider;
        [SerializeField] protected Image decayImage;
        [SerializeField] protected Image fillImage;
        //
    
        protected IDamageable AttachedDamageable; //The damageable component this health bar is dedicated to
    
        public void Start()
        {
            AttachedDamageable ??= GetComponentInParent<IDamageable>();
            
            //Destroy the health bar if there is no IDamageable component attached and print a warning.
            if (AttachedDamageable == null)
            {
                //Debug.LogWarning("No attached IDamageable component! Removing healthbar...");
                Destroy(gameObject);
                return;
            }
            //

            //Initialize slider values
            healthSlider.maxValue = AttachedDamageable.MaxHealth;
            healthSlider.value = AttachedDamageable.CurrentHealth;

            //_attachedDamageable.OnHealthChangedOwner += (float value) => UpdateUI(); //Set the UI to be updated whenever the health is changed
        }

        private void Update()
        {
            lagBehindSlider.value = Mathf.MoveTowards(lagBehindSlider.value, healthSlider.value/healthSlider.maxValue, 0.5f * Time.deltaTime);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (AttachedDamageable is GameEntity gameEntity)
            {
                healthSlider.maxValue = gameEntity.NetworkMaxHealth; //Update the slider values.
                healthSlider.value = gameEntity.NetworkCurrentHealth + gameEntity.ClientHealthDisparity;
                
                fillImage.color = gameEntity.IsInvulnerable
                    ? new Color(146f/255f, 232f/255f, 192f/255f)//new Color(0.572549f, 0.909804f, 0.7529413f)
                    : (/*gameEntity.Team == Team.Player*/true ? new Color(0.3882353f, 0.6705883f, 0.2470588f) : new Color(47f/255f, 87f/255f, 83f/255f));
                
                var dmg = PoisonEffect.GetPoisonDamage(gameEntity);

                if (decaySlider != null)
                {
                    decaySlider.maxValue = gameEntity.NetworkCurrentHealth;
                    decaySlider.value = dmg;
                }
            }
            else if (AttachedDamageable != null)
            {
                healthSlider.maxValue = AttachedDamageable.MaxHealth; //Update the slider values.
                healthSlider.value = AttachedDamageable.CurrentHealth;
            }
        }
    }
}
