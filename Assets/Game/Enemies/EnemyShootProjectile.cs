using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyShootProjectile : MonoBehaviour
{
    //Editor variables
    [SerializeField] private float projectileTravelSpeed;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Enemy enemy;
    //
    
    private Transform Target => enemy.Target;
    
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    public void ShootProjectile() //Called through animation events
    {
        var myPosition = transform.position;
        Vector2 direction = (Target.transform.position - myPosition).normalized; //Calculates direction vector
        var projectile = Instantiate(projectilePrefab); //Instantiates the projectile
        projectile.transform.position = myPosition; //Set the projectile's position to our enemy's position
        var rb = projectile.GetComponent<Rigidbody2D>();
        projectile.transform.up = direction; //Set the projectile's direction
        rb.velocity = direction * projectileTravelSpeed; //Set the projectile's velocity
    }
}
