using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class RandomAnimatorOffset : MonoBehaviour
    {
        public Vector2 offsetRange;
        public bool randomSpriteFlip;
        
        private float _activateTime;
        private Animator _animator;

        private void Awake()
        {
            _activateTime = Time.time + Random.Range(offsetRange.x, offsetRange.y);
            _animator = GetComponent<Animator>();
            _animator.enabled = false;

            if (randomSpriteFlip)
            {
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.flipX = Random.Range(0f, 1f) > 0.5f;
            }
        }

        private void Update()
        {
            if (Time.time < _activateTime) return;
            _animator.enabled = true;
            Destroy(this);
        }
    }
}