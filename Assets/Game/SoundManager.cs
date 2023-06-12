using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

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
        public List<GameSound> gameSounds = new();
        private static readonly Dictionary<string, AudioClip> AudioClips = new();

        private static bool _initialized;

        public void Awake()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            foreach (var s in gameSounds)
            {
                AudioClips.Add(s.name, s.audioClip);
            }
        }

        public static void PlaySound(AudioClip audioClip, float volume, Vector2 position, float pitch = 1, bool global = false, AudioMixerGroup mixerGroup = null) =>
            PlaySound(AudioClips.FirstOrDefault(kv => kv.Value == audioClip).Key, volume, position, pitch, global, mixerGroup);
        
        public static void PlaySound(string name, float volume, Vector2 position, float pitch = 1, bool global = false, AudioMixerGroup mixerGroup = null)
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

            var token = new CancellationTokenSource();
            token.RegisterRaiseCancelOnDestroy(audio);
            
            SetAudioMuteOnPredicate(
                    () => Vector2.Distance(GameManager.MainCamera.transform.position, position) > 50f && !global, audio)
                .AttachExternalCancellation(token.Token);

            destroy.seconds = soundClip.length * 2f;

            audio.outputAudioMixerGroup = GameManager.Instance == null ? null : mixerGroup == null ? GameManager.Instance.mainMixerGroup : mixerGroup;
            audio.clip = soundClip;
            audio.playOnAwake = false;
            audio.loop = false;
            audio.pitch = pitch;
            audio.volume = volume;

            audio.Play();
        }

        private static async UniTask SetAudioMuteOnPredicate(Func<bool> predicate, AudioSource source)
        {
            while (source.gameObject != null)
            {
                source.mute = predicate.Invoke();
                await UniTask.WaitForEndOfFrame(GameManager.Instance);
            }
        }

        public static void PlaySoundPitchVariant(string name, float volume, Vector2 position, float minPitch, float maxPitch)
        {
            PlaySound(name, volume, position, Random.Range(minPitch, maxPitch));
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

            _rampingPitch += 0.01f;
            _rampingPitchCooldown = 1f;

            audio.Play();
        }
    }
}