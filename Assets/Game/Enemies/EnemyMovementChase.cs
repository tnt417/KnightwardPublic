using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementChase : MonoBehaviour, IEnemyMovement
{
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private float speedMultiplier;

    void Start()
    {
        Target = FindObjectOfType<Player>().transform;
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        UpdateMovement();
        UpdateTarget();
    }

    public bool DoMovement => true;

    public void UpdateMovement()
    {
        _rigidbody2D.transform.Translate((Target.position - transform.position).normalized * speedMultiplier * Time.fixedDeltaTime);
    }

    public float SpeedMultiplier => speedMultiplier;
    public Transform Target { get; private set; }
    public void UpdateTarget()
    {
        //TODO throw new System.NotImplementedException();
    }
}
