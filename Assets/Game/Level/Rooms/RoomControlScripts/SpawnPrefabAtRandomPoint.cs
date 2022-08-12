using UnityEngine;
using Tools = TonyDev.Game.Global.Tools;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
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
