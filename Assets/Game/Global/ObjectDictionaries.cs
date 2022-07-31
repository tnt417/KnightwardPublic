using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev
{
    [Serializable]
    public class DictionaryEntry
    {
        public string key;
        public ScriptableObject value;
    }
    public class ObjectDictionaries : MonoBehaviour
    {
        [SerializeField] private List<DictionaryEntry> dictionaryEntries;
        public static readonly Dictionary<string, EnemyData> Enemies = new ();

        private void Awake()
        {
            foreach (var de in dictionaryEntries)
            {
                if(de.value is EnemyData ed)
                    Enemies.Add(de.key, ed);
            }
        }
    }
}
