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
        //
    
        public static bool Dead;
        private float _deathTimer;
    
        void Update()
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
            Player.Instance.playerAnimator.PlayDeadAnimation(); //Play death animation
            
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
            foreach (var e in FindObjectsOfType<Enemy>())
            {
                e.UpdateTarget(); //Set new targets for all enemies, so that they might switch back to the player.
            }
        }
    }
}
