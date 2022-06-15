using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Transform _target;
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private float speedMultiplier;
    
    void Start()
    {
        _target = FindObjectOfType<Player>().transform;
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        _rigidbody2D.velocity = (_target.position - transform.position).normalized * speedMultiplier;
    }
}
