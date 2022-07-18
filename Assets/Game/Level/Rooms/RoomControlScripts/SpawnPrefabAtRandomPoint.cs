using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools = TonyDev.Game.Global.Tools;

namespace TonyDev
{
    public class SpawnPrefabAtRandomPoint : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform[] spawnPoints;

        public void Awake()
        {
            var spawnPoint = Tools.SelectRandom(spawnPoints);
            Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        }
    }
}
