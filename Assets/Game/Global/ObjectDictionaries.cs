using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Global
{
    [Serializable]
    public class DictionaryEntry
    {
        public string key;
        public Object value;
    }
    public class ObjectDictionaries : MonoBehaviour
    {
        [SerializeField] private List<DictionaryEntry> dictionaryEntries;
        [SerializeField] private List<DictionaryEntry> prefabEntries;
        public static readonly Dictionary<string, EnemyData> Enemies = new ();
        public static readonly Dictionary<string, GameObject> Prefabs = new ();

        private void Awake()
        {
            foreach (var de in dictionaryEntries)
            {
                if(de.value is EnemyData ed)
                    Enemies.Add(de.key, ed);
            }
            foreach (var pe in prefabEntries)
            {
                if(pe.value is GameObject go)
                    Prefabs.Add(pe.key, go);
            }
        }
    }
}
