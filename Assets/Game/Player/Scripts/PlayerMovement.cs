using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private float speedMultiplier = 5f;

    void FixedUpdate()
    {
        //Movement code
        float dx = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        //float dx = Input.GetAxis("Horizontal");
        float dy = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        //float dy = Input.GetAxis("Vertical");
        //_rigidbody2D.transform.Translate(new Vector2(dx, dy).normalized * speedMultiplier * Time.fixedDeltaTime);
        _rigidbody2D.AddForce(new Vector2(dx, dy).normalized * speedMultiplier * Time.fixedDeltaTime);
        if (dy > 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Up;
        if (dy < 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Down;
        if (dx > 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Right;
        if (dx < 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Left;
        if (dy == 0 && dx == 0) Player.Instance.playerAnimator.PlayerAnimState = PlayerAnimState.Idle;
    }
}
