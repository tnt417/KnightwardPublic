using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
public class PlayerShootController : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    
    private float _shootCooldown = 0.1f;
    private float _shootTimer = 0;
    private float _bulletSpeed = 5f;
    private int _bulletDamage = 25;
    private bool _shooting;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }
    
    private void Update()
    {
        _shootTimer += Time.deltaTime;
        
        _shooting = Input.GetMouseButton(0);
        
        if (_shooting && _shootTimer > _shootCooldown)
        {
            Shoot(GetDirectionVector());
            _shootTimer = 0;
        }
    }

    private void Shoot(Vector2 direction)
    {
        GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletBody = go.GetComponent<Rigidbody2D>();
        bulletBody.velocity = direction * _bulletSpeed;
        
        Bullet b = go.GetComponent<Bullet>();
        b.damage = _bulletDamage;
        b.damagePlayer = false;
    }

    private Vector2 GetDirectionVector()
    {
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 myPos = transform.position;
        return (mousePos - myPos).normalized;
    }
}
