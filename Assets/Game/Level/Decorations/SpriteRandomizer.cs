using System.Collections.Generic;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    [ExecuteInEditMode]
    public class SpriteRandomizer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Sprite> choices;

        [SerializeField] private bool randomizeFlipX;
        
        private void Awake()
        {
            if (randomizeFlipX)
            {
                if(Random.Range(0, 1f) > 0.5f) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = GameTools.SelectRandom(choices);
            }
        }
    }
}
