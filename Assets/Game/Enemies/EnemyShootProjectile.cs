using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootProjectile : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float projectileTravelSpeed;
    [SerializeField] private GameObject projectilePrefab;
    // Start is called before the first frame update
    void Start()
    {
        target = FindObjectOfType<Player>().transform;
    }

    public void ShootProjectile()
    {
        Vector3 myPosition = transform.position;
        Vector2 direction = (target.transform.position - myPosition).normalized;
        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = myPosition;
        var rb = projectile.GetComponent<Rigidbody2D>();
        projectile.transform.up = direction;
        rb.velocity = direction * projectileTravelSpeed;
    }
}
