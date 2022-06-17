using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    private float speedMultiplier = 5f;
    [SerializeField] private Player player;
    
    void FixedUpdate()
    {
        //Movement code
        float dx = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        //float dx = Input.GetAxis("Horizontal");
        float dy = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        //float dy = Input.GetAxis("Vertical");
        _rigidbody2D.transform.Translate(new Vector2(dx, dy).normalized * speedMultiplier * Time.fixedDeltaTime);

        if (dy > 0) player.playerAnimator.FacingDirection = Direction.Up;
        if (dy < 0) player.playerAnimator.FacingDirection = Direction.Down;
        if (dx > 0) player.playerAnimator.FacingDirection = Direction.Right;
        if (dx < 0) player.playerAnimator.FacingDirection = Direction.Left;
        if (dy == 0 && dx == 0) player.playerAnimator.FacingDirection = Direction.None;
    }
}
