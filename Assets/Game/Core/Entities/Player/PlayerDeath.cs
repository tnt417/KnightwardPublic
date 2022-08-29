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
    
        public bool dead;
        private float _deathTimer;

        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        public override void OnStartLocalPlayer()
        {
            Player.LocalInstance.OnDeath += (float f) => DieLocal();
        }

        private void Update()
        {
            if (dead) //Runs while dead
            {
                _deathTimer += Time.deltaTime; //Tick death timer
                deathTimerText.text = Mathf.CeilToInt(deathCooldown - _deathTimer).ToString(); //Update death timer text
                if (_deathTimer >= deathCooldown)
                {
                    if(isLocalPlayer) ReviveLocal(); //Revive if cooldown is up.
                }
            }
        }
        
        private void DieLocal()
        {
            if(isLocalPlayer) CmdDie();
        }

        [Command(requiresAuthority = false)]
        private void CmdDie()
        {
            RpcDie();
        }

        [ClientRpc]
        private void RpcDie()
        {
            dead = true; //Die
            
            if (isLocalPlayer)
            {
                GameManager.Money = 0; //Reset money as a penalty for dying (LOCAL PLAYER ONLY)
                Player.LocalInstance.playerAnimator.PlayDeadAnimation(); //Play death animation
                GameManager.Instance.CmdReTargetEnemies(); //Set new targets for all enemies, so that they don't target the dead player
            }
            healthBarObject.SetActive(false); //Hide health bar
            _rb2d.simulated = false; //De-activate Rigidbody
            walkParticleSystem.Stop(); //Turn off walk particles
        }
        
        private void ReviveLocal()
        {
            if(isLocalPlayer) CmdRevive();
        }

        [Command(requiresAuthority = false)]
        private void CmdRevive()
        {
            RpcRevive();
        }

        [ClientRpc]
        private void RpcRevive()
        {
            dead = false; //Revive
            
            if (isLocalPlayer)
            {
                Player.LocalInstance.SetHealth(Player.LocalInstance.networkMaxHealth); //Fully heal the player
            }
            
            healthBarObject.SetActive(true); //Re-active the health bar
            _deathTimer = 0; //Reset the death timer
            deathTimerText.text = string.Empty; //Clear the death timer text
            _rb2d.simulated = true; //Re-activate the RigidBody
        }
    }
}
