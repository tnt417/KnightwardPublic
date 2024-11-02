using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.U2D;

namespace TonyDev
{
    public class MirrorController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        public Animator animator;

        private float _timer = 0;

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer < 0)
            {
                animator.SetBool("mirrorHit", false);
            }
        }

        public void PlayParticles()
        {
            particles.Play();
        }

        public void MirrorHit()
        {
            animator.SetBool("mirrorHit", true);
            _timer = 0.1f;
        }
    }
}
