using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class EnemyMovementPeriodicalStrafe : EnemyMoveBase
{
    [SerializeField] private float strafeDistance;
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeCooldown;
    [SerializeField] private float strafeRadius;
    [SerializeField] private Animator animator;
    private float _strafeTimer;
    private float _strafeProgress;
    private bool _strafing;
    private Vector2 _direction;

    private void StartStrafe()
    {
        animator.SetBool("shouldLand", false);
        animator.SetBool("shouldTravel",true);
        _strafing = true;
        //_direction = (Target.transform.position - transform.position).normalized;
        _direction = FindDirection();
    }

    private void EndStrafe()
    {
        _strafing = false;
        _strafeTimer = 0;
        _strafeProgress = 0;
        animator.SetBool("shouldTravel",false);
        animator.SetBool("shouldLand", true);
    }

    private Vector2 FindDirection()
    {
        var dist = Vector2.Distance(transform.position, Target.transform.position); //c
        var angleA = Mathf.Acos(
            Mathf.Clamp((Mathf.Pow(strafeDistance, 2f) + Mathf.Pow(dist, 2f) - Mathf.Pow(strafeRadius, 2f))/(2f*strafeDistance*dist), -1, 1));
        var final = Rotate((Target.transform.position - transform.position).normalized, angleA); 
        return final;
    }
    
    private static Vector2 Rotate(Vector2 v, float radians) {
        var sin = Mathf.Sin(radians);
        var cos = Mathf.Cos(radians);
         
        var tx = v.x;
        var ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    #region IEnemyMovement
    public override void UpdateMovement()
    {
        _strafeTimer += Time.fixedDeltaTime;
        if (_strafeTimer >= strafeCooldown && !_strafing)
        {
            StartStrafe();
        }
        else if (_strafing)
        {
            var strafeVector = _direction * strafeSpeed * Time.fixedDeltaTime * SpeedMultiplier;
            _strafeProgress += strafeVector.magnitude;
            transform.Translate(strafeVector);
        }

        if (_strafeProgress >= strafeDistance)
        {
            EndStrafe();
        }
    }
    #endregion
}
