using UnityEngine;

namespace TonyDev
{
    public class DestroyParentOnWallCollide : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Level"))
            {
                Destroy(gameObject.transform.parent.gameObject);
            }
        }
    }
}
