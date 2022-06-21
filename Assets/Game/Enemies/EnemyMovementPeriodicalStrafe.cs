using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class EnemyMovementPeriodicalStrafe : EnemyMoveBase
{
    //Editor variables
    [SerializeField] private float strafeDistance;
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeCooldown;
    [SerializeField] private float strafeRadius;
    [SerializeField] private Animator animator;
    //
    
    private float _strafeTimer;
    private float _strafeProgress;
    private bool _strafing;
    private Vector2 _direction;

    private void StartStrafe() //Start a strafe
    {
        //Update variables accordingly
        animator.SetBool("shouldLand", false);
        animator.SetBool("shouldTravel",true);
        _strafing = true;
        //
        
        _direction = FindDirection(); //Calculate the direction to strafe
    }

    private void EndStrafe() //End a strafe
    {
        //Update variables accordingly
        _strafing = false;
        _strafeTimer = 0;
        _strafeProgress = 0;
        animator.SetBool("shouldTravel",false);
        animator.SetBool("shouldLand", true);
        //
    }

    private Vector2 FindDirection() //Some trigonometry to find a position a certain distance away from both the player and enemy
    {
        var posA = transform.position;
        var posB = Target.transform.position;
        var dist = Vector2.Distance(posA, posB);
        var angleA = Mathf.Acos(
            Mathf.Clamp((Mathf.Pow(strafeDistance, 2f) + Mathf.Pow(dist, 2f) - Mathf.Pow(strafeRadius, 2f))/(2f*strafeDistance*dist), -1, 1));
        var final = Rotate((posB - posA).normalized, angleA); 
        return final;
    }
    
    //Helper function to rotate a vector by radians
    private static Vector2 Rotate(Vector2 v, float radians) {
        var sin = Mathf.Sin(radians);
        var cos = Mathf.Cos(radians);
         
        var tx = v.x;
        var ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    //IEnemyMovement interface code
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
