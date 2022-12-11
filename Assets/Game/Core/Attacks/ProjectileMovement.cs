using TonyDev.Game.Core.Attacks;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    public class ProjectileMovement : MonoBehaviour
    {
        private ProjectileData _data;
        
        private float Amplitude => _data.waveAmplitude;
        private float WaveLength => _data.waveLength;
        private float Distance => _data.waveDistance;
        private float _travelled;
        
        private Rigidbody2D _rb2d;

        private Vector2 _pathWaveOffset;

        public Vector2 direction;

        private void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if(_travelled < Distance)
            {
                _pathWaveOffset = transform.right * (Mathf.Sin(_travelled*(2f/WaveLength)*Mathf.PI) * (Amplitude-Amplitude/2));
            }
            
            var deltaDistance = _data.travelSpeed * Time.fixedDeltaTime;

            _travelled += deltaDistance;

            _rb2d.MovePosition(_rb2d.position + (Vector2)(deltaDistance * direction) + _pathWaveOffset);
        }

        public void Set(ProjectileData newData)
        {
            _data = newData;
        }
    }
}
