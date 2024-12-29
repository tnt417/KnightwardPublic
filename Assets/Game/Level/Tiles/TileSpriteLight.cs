using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using TileData = UnityEngine.Tilemaps.TileData;

namespace TonyDev
{
    public class TileSpriteLight : MonoBehaviour
    {
        private Tilemap tilemap;

        public Light2D light2D;
        private FieldInfo _LightCookieSprite =  typeof( Light2D ).GetField( "m_LightCookieSprite", BindingFlags.NonPublic | BindingFlags.Instance );

        void UpdateCookieSprite(Sprite sprite)
        {
            _LightCookieSprite.SetValue(light2D, sprite);
        }

        void Start()
        {
            tilemap = GetComponentInParent<Tilemap>();
            var sprite = tilemap.GetSprite(tilemap.WorldToCell(transform.position));
            
            if (sprite != null)
            {
                UpdateCookieSprite(sprite);
            }
            else
            {
                //Debug.LogWarning("No tile found at the given position or it's not a Tile type.");
            }
        }
    }
}
