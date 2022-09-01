using Mirror;
using TonyDev.Game.Global;
using UnityEngine;
using Tools = TonyDev.Game.Global.Tools;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
{
    public class RandomItemSpawner : MonoBehaviour
    {
        [SerializeField] private int count;
        [SerializeField] private LevelItemSpawner[] spawners;

        [ServerCallback]
        public void Start()
        {
            foreach (var s in spawners)
            {
                s.autoSpawn = false;
            }

            for (var i = 0; i < count; i++)
            {
                var spawner = Tools.SelectRandom(spawners);
                spawner.SpawnItem();
            }
        }
    }
}
