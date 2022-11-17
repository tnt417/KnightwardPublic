using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies;
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
        [SerializeField] private List<DictionaryEntry> prefabEntries;
        [SerializeField] private SpriteAtlas mainSpriteAtlas;
        private static readonly Dictionary<string, GameObject> Prefabs = new();
        public static readonly List<GameObject> EnemyPrefabs = new();
        private static SpriteAtlas _spriteAtlas;

        public static bool Initialized;

        private void Awake()
        {
            if (Initialized) return;
            Initialized = true;

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

            foreach (var enemyObject in Resources.LoadAll<GameObject>("Enemies").Where(go => go.CompareTag("Enemy")))
            {
                var enemyNameInvariant = enemyObject.GetComponent<Enemy>().EnemyName.ToLowerInvariant();

                Prefabs.Add(enemyNameInvariant, enemyObject);
                EnemyPrefabs.Add(enemyObject);
                NetworkClient.RegisterPrefab(enemyObject);
            }
            
            _spriteAtlas = mainSpriteAtlas;
            ItemGenerator.InitSprites();
        }

        public static GameObject GetPrefab(string name)
        {
            return string.IsNullOrEmpty(name) ? null : Prefabs.ContainsKey(name) ? Prefabs[name] : null;
        }

        public static string GetNameOfPrefab(GameObject prefab) => Prefabs.ContainsValue(prefab)
            ? Prefabs.FirstOrDefault(k => k.Value == prefab).Key
            : "";

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