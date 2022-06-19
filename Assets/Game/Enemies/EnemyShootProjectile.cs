using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyShootProjectile : MonoBehaviour
{
    private Transform Target => enemy.target;
    [SerializeField] private float projectileTravelSpeed;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    public void ShootProjectile() //Called through animation events
    {
        var myPosition = transform.position;
        Vector2 direction = (Target.transform.position - myPosition).normalized;
        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = myPosition;
        var rb = projectile.GetComponent<Rigidbody2D>();
        projectile.transform.up = direction;
        rb.velocity = direction * projectileTravelSpeed;
    }
}
