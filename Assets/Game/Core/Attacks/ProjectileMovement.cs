using TonyDev.Game.Core.Attacks;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    public class ProjectileMovement : MonoBehaviour
    {
        private ProjectileData _data;
        
        private float Amplitude => _data.waveAmplitude;
        private float Frequency => _data.waveFrequency;
        private float Distance => _data.waveDistance;
        private float _travelled;
        
        private Rigidbody2D _rb2d;

        private Vector2 _pathWaveOffset;

        private void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if(_travelled < Distance)
            {
                _pathWaveOffset = transform.right * Mathf.Sin(_travelled*Frequency*Mathf.PI) * Amplitude;
            }
            
            var deltaDistance = _data.travelSpeed * Time.fixedDeltaTime;

            _travelled += deltaDistance;
            _rb2d.MovePosition(_rb2d.position + (Vector2)(deltaDistance * transform.up) + _pathWaveOffset);
        }

        public void Set(ProjectileData newData)
        {
            _data = newData;
        }
    }
}
