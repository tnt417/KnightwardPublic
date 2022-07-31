using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private float deathCooldown;
        [SerializeField] private GameObject healthBarObject;
        [SerializeField] private TMP_Text deathTimerText;
        [SerializeField] private ParticleSystem walkParticleSystem;
        private Rigidbody2D _rb2d;
        //
    
        public static bool Dead;
        private float _deathTimer;

        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }
        
        private void Update()
        {
            if (Dead) //Runs while dead
            {
                _deathTimer += Time.deltaTime; //Tick death timer
                deathTimerText.text = Mathf.CeilToInt(deathCooldown - _deathTimer).ToString(); //Update death timer text
                if (_deathTimer >= deathCooldown)
                {
                    Revive(); //Revive if cooldown is up.
                }
            }
        }

        public void Die() //Called when the player's health drops to 0
        {
            if (Dead) return;
            Dead = true; //Die
            healthBarObject.SetActive(false); //Hide health bar
            GameManager.Money = 0;
            Player.Instance.playerAnimator.PlayDeadAnimation(); //Play death animation
            _rb2d.simulated = false;
            walkParticleSystem.Stop();
            
            foreach (var enemy in GameManager.Entities.Where(e => e is Enemy))
            {
                enemy.UpdateTarget(); //Set new targets for all enemies, so that they don't target the dead player
            }
        }

        private void Revive() //Called when the player's death timer is up.
        {
            Dead = false; //Revive
            _deathTimer = 0; //Reset the death timer
            healthBarObject.SetActive(true); //Re-active the health bar
            Player.Instance.SetHealth(Player.Instance.MaxHealth); //Fully heal the player
            deathTimerText.text = string.Empty; //Clear the death timer text
            _rb2d.simulated = true;
            foreach (var e in FindObjectsOfType<Enemy>())
            {
                e.UpdateTarget(); //Set new targets for all enemies, so that they might switch back to the player.
            }
        }
    }
}
