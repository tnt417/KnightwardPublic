using System;
using Mirror;
using TonyDev.Game.Global.Network;
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

    [Serializable]
    public struct PlayerSkin
    {
        public Color LowColor;
        public Color MidColor;
        public Color HighColor;

        public static PlayerSkin DefaultSkin = new PlayerSkin()
        {
            LowColor = new Color(0.1294f, 0.0941f, 0.1058f, 1f),
            MidColor = new Color(0.07843f, 0.0941f, 0.1804f, 1f),
            HighColor = new Color(0.172549f, 0.2078432f, 0.3019608f, 1f)
        };
        
        public static PlayerSkin YellowSkin = new PlayerSkin()
        {
            LowColor = FromRGB(206, 117, 43),
            MidColor = FromRGB(240, 181, 65),
            HighColor = FromRGB(255, 238, 131)
        };
        
        public static PlayerSkin GreenSkin = new PlayerSkin()
        {
            LowColor = FromRGB(47, 87, 83),
            MidColor = FromRGB(59, 125, 79),
            HighColor = FromRGB(99, 171, 63)
        };
        
        public static PlayerSkin BlueSkin = new PlayerSkin()
        {
            LowColor = FromRGB(76, 104, 133),
            MidColor = FromRGB(79, 164, 184),
            HighColor = FromRGB(146, 232, 192)
        };
        
        public static PlayerSkin RedSkin = new PlayerSkin()
        {
            LowColor = FromRGB(120, 29, 79),
            MidColor = FromRGB(173, 47, 69),
            HighColor = FromRGB(230, 69, 57)
        };
        
        public static PlayerSkin WhiteSkin = new PlayerSkin()
        {
            LowColor = FromRGB(163, 167, 194),
            MidColor = FromRGB(223, 224, 232),
            HighColor = FromRGB(245, 255, 232)
        };
        
        public static PlayerSkin BrownSkin = new PlayerSkin()
        {
            LowColor = FromRGB(59, 32, 39),
            MidColor = FromRGB(125, 56, 51),
            HighColor = FromRGB(171, 81, 48)
        };
        
        public static PlayerSkin PurpleSkin = new PlayerSkin()
        {
            LowColor = FromRGB(75, 29, 82),
            MidColor = FromRGB(105, 36, 100),
            HighColor = FromRGB(156, 42, 112)
        };
        
        public static PlayerSkin PinkSkin = new PlayerSkin()
        {
            LowColor = FromRGB(156, 42, 112),
            MidColor = FromRGB(204, 47, 123),
            HighColor = FromRGB(255, 82, 119)
        };
        
        public static PlayerSkin BlackSkin = new PlayerSkin()
        {
            LowColor = FromRGB(0, 0, 0),
            MidColor = FromRGB(0, 0, 0),
            HighColor = FromRGB(0, 0, 0)
        };

        private static Color FromRGB(float r, float g, float b)
        {
            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }

        public static PlayerSkin[] skins = new[]
        {
            DefaultSkin, RedSkin, YellowSkin, GreenSkin, BlueSkin, PurpleSkin, PinkSkin, BlackSkin, BrownSkin, WhiteSkin
        };
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

        [SyncVar(hook=nameof(OnSkinChanged))] private PlayerSkin _skin;

        public override void OnStartAuthority()
        {
            CmdSetSkin(CustomRoomPlayer.Local.skin);
        }

        private void OnSkinChanged(PlayerSkin oldSkin, PlayerSkin newSkin)
        {
            playerSpriteRenderer.sharedMaterial = new Material(playerSpriteRenderer.sharedMaterial);
            
            playerSpriteRenderer.sharedMaterial.SetColor("_LowColor", newSkin.LowColor);
            playerSpriteRenderer.sharedMaterial.SetColor("_MidColor", newSkin.MidColor);
            playerSpriteRenderer.sharedMaterial.SetColor("_HighColor", newSkin.HighColor);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetSkin(PlayerSkin skin)
        {
            _skin = skin;
        }

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