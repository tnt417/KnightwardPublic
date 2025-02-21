using UnityEngine;

namespace TonyDev
{
    public class ShadowCloneController : MonoBehaviour
    {
        private Vector2 _goalPos;
        public float deploySpeed;
        
        public void Set(Vector2 direction, float distance)
        {
            _goalPos = (Vector2)transform.position + distance * direction.normalized;
        }

        public void Swap(Vector2 newPos)
        {
            transform.position = newPos;
            _goalPos = newPos;
        }

        private void Update()
        {
            transform.position = Vector2.MoveTowards(transform.position, _goalPos, deploySpeed * Time.deltaTime);
        }
    }
}
