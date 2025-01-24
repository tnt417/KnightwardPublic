using System.Linq;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.Serialization;

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
    
        [SyncVar (hook = nameof(DeadHook))] public bool dead;
        private float _deathTimer;
        
        public bool disableDeathHandling;

        private void DeadHook(bool oldVal, bool isDead)
        {
            healthBarObject.SetActive(!isDead); //Hide health bar
            _rb2d.simulated = !isDead; //De-activate Rigidbody
            if (!isDead) walkParticleSystem.Play();
            else walkParticleSystem.Stop(); //Turn on/off walk particles

            if (isDead) return;
            
            // Revive code
            _deathTimer = 0; //Reset the death timer
            deathTimerText.text = string.Empty; //Clear the death timer text
        }
        
        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        public override void OnStartLocalPlayer()
        {
            Player.LocalInstance.OnDeathOwner += _ => DieLocal();
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
            if (disableDeathHandling || !isLocalPlayer) return;

            dead = true;

            CmdMoveEnemiesFromRoom(Player.LocalInstance.currentParentIdentityLocal);
            
            ObjectSpawner.SpawnMoney((int)(GameManager.Money * 0.8f), Player.LocalInstance.transform.position, Player.LocalInstance.CurrentParentIdentity); //Drop money on the ground
            GameManager.Money = 0; //Reset money
            Player.LocalInstance.playerMovement.DoMovement = false;
            Player.LocalInstance.playerAnimator.PlayDeadAnimation(); //Play death animation
            GameManager.Instance.CmdReTargetEnemies(); //Set new targets for all enemies, so that they don't target the dead player
            GameManager.Instance.EnterArenaPhase();
        }

        [Command(requiresAuthority = false)]
        public void CmdMoveEnemiesFromRoom(NetworkIdentity roomNetId)
        {
            if (roomNetId != null)
            {
                var room = roomNetId.GetComponent<Room>();

                if (room.PlayerCount <= 1)
                {
                    foreach (var e in room.ContainedEntities.OfType<Enemy>())
                    {
                        WaveManager.Instance.MoveEnemyToWave(e);
                    }
                }
            }
        }

        public void ReviveLocal()
        {
            if (!isLocalPlayer) return;
            
            dead = false;
            
            _deathTimer = 0;
            deathTimerText.text = string.Empty;
            
            Player.LocalInstance.playerMovement.DoMovement = true;
            Player.LocalInstance.SetHealth(Player.LocalInstance.NetworkMaxHealth); //Fully heal the player
        }
    }
}
