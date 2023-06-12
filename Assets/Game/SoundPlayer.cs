using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class SoundPlayer : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 0.5f;
        public Vector2 pitchRange = new Vector2(1, 1);
        public bool playOnAwake = true;
        public bool global = false;
        public AudioMixerGroup mixerGroup;
        public float cooldown;
        private float _nextPlayTime;
        private void Awake()
        {
            if(playOnAwake) PlaySound();
        }

        public void PlaySound()
        {
            if (Time.time < _nextPlayTime) return;
            
            SoundManager.PlaySound(clip, volume, transform.position, Random.Range(pitchRange.x, pitchRange.y), global, mixerGroup);
            
            _nextPlayTime = Time.time + cooldown;
        }
    }
}
