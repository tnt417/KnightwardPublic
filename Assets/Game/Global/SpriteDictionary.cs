using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Items;
using UnityEngine;

namespace TonyDev.Game.Global
{
    [Serializable] //This struct necessary to allow dictionary editing from Unity editor
    public struct SpriteEntry
    {
        public string key;
        public Sprite sprite;

        public SpriteEntry(string key, Sprite sprite)
        {
            this.key = key;
            this.sprite = sprite;
        }
    }
    public class SpriteDictionary : MonoBehaviour
    {
        [SerializeField] private SpriteEntry[] spriteEntries; //Sole purpose is to make entries in the editor
        public static readonly Dictionary<string, Sprite> Sprites = new (); //Dictionary to make accessing sprites through code easier.
        private static bool _initialized;
        private void Awake()
        {
            if (_initialized) return;
            _initialized = true;
            
            //Add all the sprites into the dictionary
            foreach(var se in spriteEntries)
            {
                Sprites.Add(se.key, se.sprite);
            }
            //
        
            ItemGenerator.InitSprites(); //Tell the ItemGenerator that we are done with our job.
        }
    }
}