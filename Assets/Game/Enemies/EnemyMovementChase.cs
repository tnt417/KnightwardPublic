using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementChase : EnemyMoveBase
{
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private float speedMultiplier;
    public override void UpdateMovement()
    {
        rigidbody2D.transform.Translate((Target.position - transform.position).normalized * speedMultiplier * Time.fixedDeltaTime);
    }
}
