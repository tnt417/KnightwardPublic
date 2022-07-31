using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player
{
    public enum PlayerAnimState
    {
        Up, Down, Left, Right, Idle, Dead
    }

    public class PlayerAnimator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private int playerSpriteIndex;
        [SerializeField] private Sprite[] playerSprites;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private ParticleSystem playerWalkParticles;
        //

        //PlayerAnimState only structured like this to allow a method to be called when the direction is changed.
        private PlayerAnimState _playerAnimState = PlayerAnimState.Idle;
        public PlayerAnimState PlayerAnimState
        {
            get => _playerAnimState;
            set
            {
                if (_playerAnimState == value) return;
                DirectionChanged(_playerAnimState, value);
                _playerAnimState = value;
            }
        }
        //

        public void PlayHurtAnimation()
        {
            playerAnimator.Play("PlayerHurt");
        }

        private void Update()
        {
            playerSpriteRenderer.sprite = playerSprites[playerSpriteIndex];

            playerAnimator.speed = PlayerStats.GetStat(Stat.MoveSpeed) / 10f;

            //Plays different animations depending on the animation state
            switch (PlayerAnimState)
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

        //This function poorly named. All it does is pause/play the walk particles when the player is not walking/walking.
        private void DirectionChanged(PlayerAnimState oldPlayerAnimState, PlayerAnimState newPlayerAnimState)
        {
            if(newPlayerAnimState == PlayerAnimState.Idle) playerWalkParticles.Stop();
            else playerWalkParticles.Play();
        }

        //Sets the anim state to dead
        public void PlayDeadAnimation()
        {
            _playerAnimState = PlayerAnimState.Dead;
        }
    }
}