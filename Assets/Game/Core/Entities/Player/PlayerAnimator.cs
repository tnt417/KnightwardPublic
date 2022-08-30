using Mirror;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player
{
    public enum PlayerAnimState
    {
        Up,
        Down,
        Left,
        Right,
        Idle,
        Dead
    }

    public class PlayerAnimator : NetworkBehaviour
    {
        //Editor variables
        [SerializeField] private Sprite[] playerSprites;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private PlayerAnimationValues playerAnimationValues;
        [SerializeField] private ParticleSystem playerWalkParticles;
        //

        private int PlayerSpriteIndex => playerAnimationValues.spriteIndex;
        
        //Custom setter to allow controlling of walk particles without spamming Play and Stop on the particle system.
        [SyncVar] private PlayerAnimState _playerAnimState = PlayerAnimState.Idle;

        public PlayerAnimState PlayerAnimState
        {
            get => _playerAnimState;
            set
            {
                if (_playerAnimState == value || !hasAuthority) return;
                DirectionChanged(_playerAnimState, value);
                _playerAnimState = value;
                CmdSetAnimState(_playerAnimState);
            }
        }
        //

        private void Update()
        {
            playerSpriteRenderer.sprite = playerSprites[PlayerSpriteIndex];

            playerAnimator.speed = PlayerStats.Stats.GetStat(Stat.MoveSpeed) / 10f;

            //Plays different animations depending on the animation state
            switch (_playerAnimState)
            {
                case PlayerAnimState.Up:
                    playerAnimator.Play("WalkUp");
                    break;
                case PlayerAnimState.Down:
                    playerAnimator.Play("WalkDown");
                    break;
                case PlayerAnimState.Left:
                    playerAnimator.Play("WalkLeft");
                    break;
                case PlayerAnimState.Right:
                    playerAnimator.Play("WalkRight");
                    break;
                case PlayerAnimState.Idle:
                    playerAnimator.Play("Idle");
                    break;
                case PlayerAnimState.Dead:
                    playerAnimator.Play("PlayerDead");
                    break;
            }
            //
        }

        [Command(requiresAuthority = false)]
        public void CmdSetAnimState(PlayerAnimState animState)
        {
            _playerAnimState = animState;
        }

        //This function poorly named. All it does is pause/play the walk particles when the player is not walking/walking.
        private void DirectionChanged(PlayerAnimState oldPlayerAnimState, PlayerAnimState newPlayerAnimState)
        {
            if (newPlayerAnimState == PlayerAnimState.Idle) playerWalkParticles.Stop();
            else playerWalkParticles.Play();
        }

        //Sets the anim state to dead
        public void PlayDeadAnimation()
        {
            PlayerAnimState = PlayerAnimState.Dead;
        }
    }
}