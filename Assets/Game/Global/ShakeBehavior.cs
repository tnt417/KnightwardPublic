using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class ShakeBehavior : MonoBehaviour
    {
        public bool shakeOnAwake = true;
        public float intensity = 10f;
        public float speed = 2f;

        public void Awake()
        {
            if (shakeOnAwake) Shake();
        }

        public void Shake()
        {
            SmoothCameraFollow.Shake(intensity, speed);   
        }
    }
}
