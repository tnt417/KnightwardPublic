using UnityEngine;

namespace TonyDev
{
    public class AlignToPixelGrid : MonoBehaviour
    {
        private void Start()
        {
            transform.position = RoundToNearestSixteenth(transform.position);
        }
        
        Vector2 RoundToNearestSixteenth(Vector2 input)
            {
                return new Vector2(
                    Mathf.Round(input.x * 16) / 16,
                    Mathf.Round(input.y * 16) / 16
                );
            }
    }
}
