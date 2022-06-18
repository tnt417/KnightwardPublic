using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementPeriodicalStrafe : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float strafeDistance;
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeCooldown;
    [SerializeField] private Animator animator;
    private float strafeTimer;
    private float strafeProgress;
    private bool strafing;
    private Vector2 direction;
    
    private void Start()
    {
        Target = FindObjectOfType<Player>().transform;
    }
    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void StartStrafe()
    {
        animator.SetBool("shouldLand", false);
        animator.SetBool("shouldTravel",true);
        strafing = true;
        direction = (Target.transform.position - transform.position).normalized;
    }

    private void EndStrafe()
    {
        strafing = false;
        strafeTimer = 0;
        strafeProgress = 0;
        animator.SetBool("shouldTravel",false);
        animator.SetBool("shouldLand", true);
    }

    #region IEnemyMovement
    public bool DoMovement { get; }
    public void UpdateMovement()
    {
        strafeTimer += Time.fixedDeltaTime;
        if (strafeTimer >= strafeCooldown && !strafing)
        {
            StartStrafe();
        }
        else if (strafing)
        {
            var strafeVector = Vector2.Perpendicular(direction) * strafeSpeed * Time.fixedDeltaTime;
            strafeProgress += strafeVector.magnitude;
            transform.Translate(strafeVector);
        }

        if (strafeProgress >= strafeDistance)
        {
            EndStrafe();
        }
    }

    public float SpeedMultiplier { get; }
    public Transform Target { get; private set; }
    public void UpdateTarget()
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
