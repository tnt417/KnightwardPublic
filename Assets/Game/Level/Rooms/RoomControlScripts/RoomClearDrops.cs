using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class RoomClearDrops : MonoBehaviour
    {
        private float ChestSpawnChance => 0.1f * NetworkServer.connections.Count;

        private Room _parentRoom;
        
        [ServerCallback]
        private void Start()
        {
            _parentRoom = transform.root.GetComponent<Room>();
            _parentRoom.onRoomClearServer.AddListener(OnClearServer);
        }

        static RoomClearDrops()
        {
            GameManager.OnGameManagerAwake += () => _chestRollAttempts = 0;
        }
        
        private bool _cleared = false;

        private static int _chestRollAttempts = 0;
        
        [Server]
        private void OnClearServer()
        {
            if (_cleared) return;
            _cleared = true;
        
            var originPos = transform.position;

            _chestRollAttempts =
                GameTools.RollChanceProgressive(ChestSpawnChance, out var success, _chestRollAttempts, 0.02f); //Chance increases by 2% for every failed attempt
            
            if (success)
            {
                ObjectSpawner.SpawnChest(0, originPos, _parentRoom.netIdentity);
            }

            var healthSpawned = Random.Range(3 + (int)(GameManager.EnemyDifficultyScale / 5), 4 + (int)(GameManager.EnemyDifficultyScale / 5));
            
            for (float i = 0; i < healthSpawned; i++)
            {
                GameManager.Instance.SpawnHealthPickupOnEach(new Vector2(originPos.x + 2*Mathf.Cos(i/healthSpawned*2*Mathf.PI), originPos.y + 2*Mathf.Sin(i/healthSpawned*2*Mathf.PI)), _parentRoom.netIdentity);
            }

            var moneyScale = ItemGenerator.GenerateCost(ItemRarity.Common, GameManager.DungeonFloor);
            
            GameManager.Instance.CmdSpawnMoney(Mathf.CeilToInt(Random.Range(moneyScale/7f, moneyScale/6f)), transform.position, _parentRoom.netIdentity);
        }
    }
}
