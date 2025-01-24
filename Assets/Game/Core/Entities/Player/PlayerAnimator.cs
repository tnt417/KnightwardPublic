using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level;
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
        [SerializeField] private SpriteRenderer[] playerSpriteRenderers;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private PlayerAnimationValues playerAnimationValues;
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private SpriteRenderer weaponSpriteRenderer;

        [SerializeField] private ParticleSystem playerWalkParticles;
        //

        [NonSerialized] public string attackAnimationName = "Attack";

        [SyncVar (hook = nameof(WeaponSpriteUpdateHook))] [NonSerialized] private string _weaponSpriteName = "broadsword_hand";

        public void SetAttackAnimProgress(float normalized)
        {
            Player.LocalInstance.SetAttackProgress(normalized);
            playerAnimator.Play(playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, normalized);
        }

        public override void OnStartClient()
        {
            weaponSpriteRenderer.sprite = ObjectFinder.GetSprite("broadsword_hand");
        }

        private void WeaponSpriteUpdateHook(string oldSprite, string newSprite)
        {
            weaponSpriteRenderer.sprite = ObjectFinder.GetSprite(newSprite);
        }

        public void SetWeaponAnimSprite(string spriteName)
        {
            if (!isOwned)
            {
                Debug.LogWarning("SetWeaponAnimSprite called on non-owner!");
                return;
            }
            
            weaponSpriteRenderer.sprite = ObjectFinder.GetSprite(spriteName);
            _weaponSpriteName = spriteName;
        }
        
        private int PlayerSpriteIndex => playerAnimationValues.spriteIndex;

        //Custom setter to allow controlling of walk particles without spamming Play and Stop on the particle system.
        [SyncVar] private PlayerAnimState _playerAnimState = PlayerAnimState.Idle;

        public PlayerAnimState PlayerAnimState
        {
            get => _playerAnimState;
            set
            {
                if (_playerAnimState == value || !authority) return;
                DirectionChanged(_playerAnimState, value);
                _playerAnimState = value;
                SetAnimState(_playerAnimState);
            }
        }
        //

        private float _playerScale = 1;

        public void ModifyPlayerSize(float multiplier)
        {
            _playerScale *= multiplier;
            playerAnimator.transform.localScale = _playerScale * Vector3.one;
        }

        [SyncVar(hook = nameof(OnSkinChanged))]
        private PlayerSkin _skin;
        
        [SyncVar]
        private int _playerNum;

        public override void OnStartAuthority()
        {
            SetSkin(CustomRoomPlayer.Local.skin);
            SetPlayerNum(CustomRoomPlayer.Local.playerNumber);
        }

        private void OnSkinChanged(PlayerSkin oldSkin, PlayerSkin newSkin)
        {
            foreach (var playerSpriteRenderer in playerSpriteRenderers)
            {
                playerSpriteRenderer.sharedMaterial = new Material(playerSpriteRenderer.sharedMaterial);

                playerSpriteRenderer.sharedMaterial.SetColor("_LowColor", newSkin.LowColor);
                playerSpriteRenderer.sharedMaterial.SetColor("_MidColor", newSkin.MidColor);
                playerSpriteRenderer.sharedMaterial.SetColor("_HighColor", newSkin.HighColor);
            }
        }

        [Client]
        public void SetSkin(PlayerSkin skin)
        {
            if (!isOwned)
            {
                Debug.LogWarning("Called without ownership!");
                return;
            }

            OnSkinChanged(default, skin);
            
            _skin = skin;
        }
        
        [Client]
        public void SetPlayerNum(int num)
        {
            if (!isOwned)
            {
                Debug.LogWarning("Called without ownership!");
                return;
            }
            
            _playerNum = num;
        }

        public void SetOpacity(float value)
        {
            var c = new Color(1.0f, 1.0f, 1.0f, value);
            
            for(var i = 0; i < playerSpriteRenderers.Length; i++)
            {
                playerSpriteRenderers[i].color = c;
            }
        }

        public bool isInLadderAnim = false;
        
        public async UniTask JumpIntoLadderTask(GameObject ladder, bool regen)
        {
            var goingToArena = GameManager.GamePhase == GamePhase.Dungeon && !regen;
            
            isInLadderAnim = true;
            
            _overriding = true;
            Player.LocalInstance.playerMovement.DoMovement = false;
            Player.LocalInstance.stopPlayerAttack = true;
                
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            playerAnimator.Play(goingToArena ? "LadderClimb" : "JumpIntoLadder");
            
            var initial = Time.time;

            while (Time.time - initial < 0.3333f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                Player.LocalInstance.transform.position = Vector2.MoveTowards(
                    Player.LocalInstance.transform.position,
                    ladder.transform.position, Time.deltaTime * 5f);
            }

            PlayerSpawnAnim().Forget();
        }

        private float _storedNormalizedAttackTime = 0;
        
        private void PlayInAllLayers(string anim, bool attackNormalized = false)
        {
            if (!isOwned || _overriding) return;

            _storedNormalizedAttackTime = Player.LocalInstance.NormalizedAttackTime;
            
            playerAnimator.Play(anim, 0,
                attackNormalized
                    ? (_storedNormalizedAttackTime / 2 - 0.03f) % 1f
                    : _playerMovementTime / (21/60f) * playerAnimator.GetFloat("moveSpeed") % 1f); // Subtracting 0.03f to make the animations feel more in sync
            playerAnimator.Play(anim, 1, (_playerMovementTime / (21/60f) * playerAnimator.GetFloat("moveSpeed")) % 1f);
        }

        private bool _overriding = false;
        
        public async UniTask PlayOverrideAnim(string animName, int layer, float normalized, float delay)
        {
            _overriding = true;
            playerAnimator.Play(animName, layer, normalized);
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            _overriding = false;
        }

        private PlayerAnimState _lastAnimState = PlayerAnimState.Dead;
        private PlayerAnimState _lastDirectionState = PlayerAnimState.Down;
        private bool _lastAttack = false;

        private float _playerMovementTime;
        private float _lastPlayerMoveTime;

        public bool attackingOverride = false;

        public void Shake(float speed)
        {
            playerAnimator.SetFloat("shakeSpeed", speed);
            playerAnimator.Play("Shake", 3);
        }

        public void UnShake()
        {
            playerAnimator.Play("NotShake", 3);
        }
        
        public override void OnStartLocalPlayer()
        {
            base.OnStartServer();
            PlayerSpawnAnim(true, true).Forget();
        }
        
        public async UniTask PlayerSpawnAnim(bool focusCrystal = false, bool scaleDelayWithPlayers = false)
        {
            _overriding = true;
            Player.LocalInstance.stopPlayerAttack = true;
            Player.LocalInstance.playerMovement.DoMovement = false;
            if(focusCrystal) GameManager.Instance.doCrystalFocusing = true;
            await UniTask.WaitUntil(() => _playerNum > 0);
            if(scaleDelayWithPlayers) await UniTask.Delay(TimeSpan.FromSeconds(0.5f * _playerNum));
            playerAnimator.Play("PlayerSpawnInitial");
            await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
            if(focusCrystal) GameManager.Instance.doCrystalFocusing = false;
            _lastAnimState = PlayerAnimState.Dead;
            Player.LocalInstance.playerMovement.DoMovement = true;
            Player.LocalInstance.stopPlayerAttack = false;
            _overriding = false;
            isInLadderAnim = false;
        }

        private void Update()
        {
            //playerSpriteRenderers[0].sprite = playerSprites[PlayerSpriteIndex];

            playerAnimator.SetFloat("moveSpeed", PlayerStats.LocalStats.GetStat(Stat.MoveSpeed));
            playerAnimator.SetFloat("attackSpeed", PlayerStats.LocalStats.GetStat(Stat.AttackSpeed));

            if (PlayerAnimState == _lastAnimState && _lastAttack == (Player.LocalInstance.CanAttack || attackingOverride)) return;
            _lastAnimState = PlayerAnimState;
            _lastAttack = Player.LocalInstance.CanAttack || attackingOverride;

            if (Player.LocalInstance.playerMovement.currentMovementInput != Vector2.zero)
            {
                _lastPlayerMoveTime = Time.time;
            }

            if (Time.time - _lastPlayerMoveTime > 0.25f)
            {
                _playerMovementTime = 0;
            }
            else
            {
                _playerMovementTime += Time.deltaTime;
            }

            if (_playerAnimState == PlayerAnimState.Dead)
            {
                PlayInAllLayers("PlayerDead");
                return;
            }

            if (PlayerAnimState != PlayerAnimState.Idle && PlayerAnimState != PlayerAnimState.Dead)
            {
                _lastDirectionState = PlayerAnimState;
            }
            
            SetFlip(_lastDirectionState == PlayerAnimState.Left);

            if (Player.LocalInstance.CanAttack || attackingOverride)
            {
                var directionNameAddon = _lastDirectionState switch
                {
                    PlayerAnimState.Left => "Side",
                    PlayerAnimState.Right => "Side",
                    PlayerAnimState.Up => "Back",
                    PlayerAnimState.Down => "Down"
                };
                var animName = "Player" + attackAnimationName + directionNameAddon +
                               (_playerAnimState == PlayerAnimState.Idle ? "Idle" : "Move");
                
                PlayInAllLayers(animName, true);
            }
            else
            {
                PlayInAllLayers(_playerAnimState switch
                    {
                        PlayerAnimState.Left => "WalkLeft",
                        PlayerAnimState.Right => "WalkRight",
                        PlayerAnimState.Up => "WalkUp",
                        PlayerAnimState.Down => "WalkDown",
                        PlayerAnimState.Idle => "Idle" + Enum.GetName(typeof(PlayerAnimState), _lastDirectionState)
                    },
                    false);
            }
        }

        private void SetFlip(bool flip)
        {
            foreach (var sr in playerSpriteRenderers)
            {
                sr.flipX = flip;
            }
            weaponTransform.localScale = new Vector2(flip ? -1 : 1, 1);
        }

        //[Command(requiresAuthority = false)]
        [Client]
        public void SetAnimState(PlayerAnimState animState)
        {
            if (!isOwned)
            {
                Debug.LogWarning("Player anim state set without ownership!");
                return;
            }
            
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