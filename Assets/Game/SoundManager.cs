using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
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
        private static Dictionary<string, AudioClip> _audioClips;

        public void Awake()
        {
            foreach (var s in gameSounds)
            {
                _audioClips.Add(s.name, s.audioClip);
            }
        }
        
        public static void PlaySound(string name, Vector2 position)
        {
            var soundClip = _audioClips[name];
            //TODO
        }
    }
}