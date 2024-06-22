using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    public class PoisonBombController : MonoBehaviour
    {
        public Vector2 direction;
        public float speed;
        public Animator animator;
        private Rigidbody2D rb2d;

        private void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        public void SetIntial(Vector2 initialDirection, float initialSpeed)
        {
            direction = initialDirection;
            speed = initialSpeed;
        }

        private void FixedUpdate()
        {
            speed -= 2f * Time.fixedDeltaTime;
            speed = Mathf.Clamp(speed, 0, Mathf.Infinity);
            
            animator.SetFloat("speed", speed);

            rb2d.MovePosition((Vector2)transform.position + speed*direction.normalized*Time.fixedDeltaTime);
            
            var hit = Physics2D.Raycast(rb2d.position, direction, 0.5f, LayerMask.GetMask("Level"));

            if (hit.collider != null)
            {
                speed -= 1f;
                direction = Vector2.Reflect(direction, hit.normal);
            }
        }
    }
}
