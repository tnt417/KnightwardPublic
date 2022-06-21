using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    //Editor variables
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private float speedMultiplier;
    //
    
    [NonSerialized] public bool DoMovement = true;

    private void FixedUpdate()
    {
        if (!DoMovement) return;
        
        //1, 0, or -1 depending on keys being pressed
        float dx = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float dy = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        
        //Add force, applying the movement.
        rb2D.AddForce(new Vector2(dx, dy).normalized * speedMultiplier * Time.fixedDeltaTime * (1f+PlayerStats.GetStatBonus(Stat.MoveSpeed)));
        
        //Set animation directions based on the keys that were pressed.
        if (dy > 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Up;
        if (dy < 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Down;
        if (dx > 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Right;
        if (dx < 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Left;
        if (dy == 0 && dx == 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Idle;
        //
    }
}
