using System;
using System.Collections.Generic;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game
{
    [Serializable]
    public struct GameSound
    {
        public string name;
        public AudioClip audioClip;
    }
    
    public class SoundManager : MonoBehaviour
    {
        public List<GameSound> gameSounds = new ();
        private static readonly Dictionary<string, AudioClip> AudioClips = new ();

        private static bool _initialized;
        
        public void Awake()
        {
            if (_initialized) return;
            _initialized = true;
            
            foreach (var s in gameSounds)
            {
                AudioClips.Add(s.name, s.audioClip);
            }
        }
        
        public static void PlaySound(string name, Vector2 position)
        {
            var soundClip = AudioClips[name];
            
            var go = new GameObject
            {
                transform =
                {
                    position = position
                }
            };

            var audio = go.AddComponent<AudioSource>();
            var destroy = go.AddComponent<DestroyAfterSeconds>();
            
            destroy.seconds = soundClip.length * 2f;
            
            audio.clip = soundClip;
            audio.playOnAwake = false;
            audio.loop = false;
            
            audio.Play();
        }

        private void Update()
        {
            _rampingPitchCooldown -= Time.deltaTime;

            if (_rampingPitchCooldown <= 0)
            {
                _rampingPitch = 1f;
            }
            
            _rampingPitch = Mathf.Clamp(_rampingPitch, 1f, 1.75f);
        }

        private static float _rampingPitch = 1f;
        private static float _rampingPitchCooldown = 1f;
        
        public static void PlayRampingPitchSound(string name, Vector2 position)
        {
            var soundClip = AudioClips[name];
            
            var go = new GameObject
            {
                transform =
                {
                    position = position
                }
            };

            var audio = go.AddComponent<AudioSource>();
            var destroy = go.AddComponent<DestroyAfterSeconds>();

            destroy.seconds = soundClip.length * 2f;
            audio.clip = soundClip;
            audio.pitch = _rampingPitch;
            audio.playOnAwake = false;
            audio.loop = false;

            _rampingPitch+=0.01f;
            _rampingPitchCooldown = 1f;
            
            audio.Play();
        }
    }
}