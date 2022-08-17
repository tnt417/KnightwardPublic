using System.Linq;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player
{
    public class PlayerDeath : NetworkBehaviour
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
            if (!isLocalPlayer) return;
            
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
            if (Dead || !isLocalPlayer) return; //Don't re-die if already dead
            Dead = true; //Die
            healthBarObject.SetActive(false); //Hide health bar
            GameManager.Money = 0; //Reset money as a penalty for dying
            Player.LocalInstance.playerAnimator.PlayDeadAnimation(); //Play death animation
            _rb2d.simulated = false; //De-activate Rigidbody
            walkParticleSystem.Stop(); //Turn off walk particles
            
            foreach (var enemy in GameManager.Entities.Where(e => e is Enemy))
            {
                enemy.UpdateTarget(); //Set new targets for all enemies, so that they don't target the dead player
            }
        }

        private void Revive() //Called when the player's death timer is up.
        {
            if (!isLocalPlayer) return;
            
            Dead = false; //Revive
            _deathTimer = 0; //Reset the death timer
            healthBarObject.SetActive(true); //Re-active the health bar
            Player.LocalInstance.SetHealth(Player.LocalInstance.MaxHealth); //Fully heal the player
            deathTimerText.text = string.Empty; //Clear the death timer text
            _rb2d.simulated = true; //Re-activate the RigidBody
        }
    }
}
