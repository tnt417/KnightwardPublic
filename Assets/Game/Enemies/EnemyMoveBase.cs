using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyMoveBase : MonoBehaviour, IEnemyMovement
{
    private Enemy _enemy;
    private void Start()
    {
        _enemy = GetComponent<Enemy>();
        _enemy.UpdateTarget();
    }
    private void FixedUpdate()
    {
        UpdateMovement();
    }

    public bool DoMovement { get; } = true;
    public abstract void UpdateMovement();

    public float SpeedMultiplier { get; } = 1;
    public Transform Target => _enemy.target;
}
