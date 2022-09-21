using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Global
{
    [Serializable]
    public class DictionaryEntry
    {
        public string key;
        public Object value;
    }
    public class ObjectFinder : MonoBehaviour
    {
        [SerializeField] private List<DictionaryEntry> dictionaryEntries;
        [SerializeField] private List<DictionaryEntry> prefabEntries;
        [SerializeField] private List<DictionaryEntry> animatorEntries;
        [SerializeField] private SpriteAtlas mainSpriteAtlas;
        private static readonly Dictionary<string, EnemyData> Enemies = new ();
        private static readonly Dictionary<string, GameObject> Prefabs = new ();
        private static SpriteAtlas _spriteAtlas;

        private static bool _initialized;
        
        private void Awake()
        {
            if (_initialized) return;
            _initialized = true;
            
            foreach (var de in dictionaryEntries)
            {
                if(de.value is EnemyData ed)
                    Enemies.Add(de.key.ToLowerInvariant(), ed);
            }
            foreach (var pe in prefabEntries)
            {
                if (pe.value is GameObject go)
                {
                    if (go.GetComponent<NetworkIdentity>() != null)
                    {
                        NetworkClient.RegisterPrefab(go);
                    }
                    
                    Prefabs.Add(pe.key, go);
                }
            }

            _spriteAtlas = mainSpriteAtlas;
            ItemGenerator.InitSprites();
        }

        public static EnemyData GetEnemyData(string name) => Enemies.ContainsKey(name.ToLowerInvariant()) ? Enemies[name.ToLowerInvariant()] : null;

        public static GameObject GetPrefab(string name) => string.IsNullOrEmpty(name) ? null : Prefabs.ContainsKey(name) ? Prefabs[name] : null;
        public static string GetNameOfPrefab(GameObject prefab) => Prefabs.ContainsValue(prefab) ? Prefabs.FirstOrDefault(k => k.Value == prefab).Key : "";

        public static Sprite GetSprite(string name) => _spriteAtlas.GetSprite(name);

        public static Sprite[] GetSpritesWithPrefix(string prefix)
        {
            var sprites = new Sprite[_spriteAtlas.spriteCount];
            _spriteAtlas.GetSprites(sprites);

            sprites = sprites.Where(s => s.name.StartsWith(prefix)).ToArray();

            return sprites;
        }
    }
}
