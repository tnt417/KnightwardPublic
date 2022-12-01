using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game;
using UnityEngine;

namespace TonyDev
{
    public class SoundPlayer : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 0.5f;
        private void Start()
        {
            SoundManager.PlaySound(clip, volume, transform.position);
        }
    }
}
