using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SubsystemsImplementation;

public enum Direction
{
    Up, Down, Left, Right, None
}

public class PlayerAnimator : MonoBehaviour
{

    [SerializeField] private int playerSpriteIndex;
    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private ParticleSystem playerWalkParticles;

    private Direction _direction = Direction.None;
    public Direction FacingDirection
    {
        get => _direction;
        set
        {
            if (_direction == value) return;
            DirectionChanged(_direction, value);
            _direction = value;
        }
    }

    public void PlayHurtAnimation()
    {
        playerAnimator.Play("PlayerHurt");
    }

    private void Update()
    {
        playerSpriteRenderer.sprite = playerSprites[playerSpriteIndex];

        switch (FacingDirection)
        {
            case Direction.Up:
                playerAnimator.Play("WalkUp");
                break;
            case Direction.Down:
                playerAnimator.Play("WalkDown");
                break;
            case Direction.Left:
                playerAnimator.Play("WalkLeft");
                break;
            case Direction.Right:
                playerAnimator.Play("WalkRight");
                break;
            case Direction.None:
                playerAnimator.Play("Idle");
                break;
        }
    }

    private void DirectionChanged(Direction oldDirection, Direction newDirection)
    {
        if(newDirection == Direction.None) playerWalkParticles.Stop();
        else playerWalkParticles.Play();
    }
}
