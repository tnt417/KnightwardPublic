using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SubsystemsImplementation;

public enum PlayerAnimState
{
    Up, Down, Left, Right, Idle, Dead
}

public class PlayerAnimator : MonoBehaviour
{

    [SerializeField] private int playerSpriteIndex;
    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private ParticleSystem playerWalkParticles;

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

    public void PlayHurtAnimation()
    {
        playerAnimator.Play("PlayerHurt");
    }

    private void Update()
    {
        playerSpriteRenderer.sprite = playerSprites[playerSpriteIndex];

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
    }

    private void DirectionChanged(PlayerAnimState oldPlayerAnimState, PlayerAnimState newPlayerAnimState)
    {
        if(newPlayerAnimState == PlayerAnimState.Idle) playerWalkParticles.Stop();
        else playerWalkParticles.Play();
    }

    public void PlayDeadAnimation()
    {
        _playerAnimState = PlayerAnimState.Dead;
    }
}
