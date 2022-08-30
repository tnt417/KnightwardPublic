using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Core.Items.Money;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations.Chests;
using TonyDev.Game.Level.Rooms;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Global
{
    public class ObjectSpawner : MonoBehaviour
    {
        private static ObjectSpawner _instance;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private float moneyOutwardForce;
        [SerializeField] private int objectPoolSize;
        private static readonly Queue<GameObject> PopupObjectPool = new ();
        
        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        public static void SpawnPopup(Vector2 position, int damage, bool critical)
        {
            Debug.Log("Spawning popup!");
            var go = PopupObjectPool.Count >= _instance.objectPoolSize ? PopupObjectPool.Dequeue() : Instantiate(_instance.damageNumberPrefab, position, Quaternion.identity);

            go.transform.position = position;
            
            var tmp = go.GetComponentInChildren<TextMeshPro>();
            var anim = go.GetComponent<Animator>();
            
            anim.Play("DamagePopup");

            tmp.text = (-damage).ToString(); //Negative, so positive damage has a '-' and healing doesn't.
            
            //Yellow = normal damage
            //Red = crit damage
            //Green = normal healing
            //Magenta = crit healing
            tmp.color = damage > 0 ? critical ? Color.red : Color.yellow : critical ? Color.magenta : Color.green;

            PopupObjectPool.Enqueue(go);
        }

        public static void SpawnMoney(int amount, Vector2 originPos, NetworkIdentity parentRoom)
        {
            for (var i = 0; i < amount; i++)
            {
                var angle = Random.Range(0, 360);

                var radAngle = angle * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
                
                var moneyObject = Instantiate(_instance.moneyPrefab, originPos, Quaternion.identity);

                if (parentRoom != null)
                {
                    var room = parentRoom.GetComponent<Room>();

                    if (room != null)
                    {
                        var money = moneyObject.GetComponent<MoneyObject>();
                        
                        money.CurrentParentIdentity = parentRoom;
                        
                        room.roomChildObjects.Add(moneyObject);
                    }
                }

                var rb2d = moneyObject.GetComponent<Rigidbody2D>();
                rb2d.velocity = dir * _instance.moneyOutwardForce;
            }
        }
        
        //All enemies should be spawned using this method.
        [Server]
        public static void SpawnEnemy(EnemyData enemyData, Vector2 position, NetworkIdentity parent)
        {
            if (enemyData == null) return;
            
            var enemy = Instantiate(enemyData.prefab == null ? _instance.enemyPrefab : enemyData.prefab, position,
                Quaternion.identity).GetComponent<Enemy>();

            NetworkServer.Spawn(enemy.gameObject);

            enemy.netIdentity.AssignClientAuthority(NetworkServer.localConnection);
            
            enemy.CmdSetEnemyData(enemyData);
            
            enemy.CmdSetParentIdentity(parent);
        }

        [GameCommand(Keyword = "spawn", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Spawned.")]
        public static void SpawnEnemyCommand(string enemyName, int amount = 1)
        {
            for (var i = 0; i < amount; i++)
                if (Camera.main is not null)
                    GameManager.Instance.CmdSpawnEnemy(enemyName,
                        Camera.main.ScreenToWorldPoint(Input.mousePosition), null);
        }

        [Server]
        public static GroundItem SpawnGroundItem(Item item, float costMultiplier, Vector2 position, NetworkIdentity parent)
        {
            var groundItemObject = Instantiate(_instance.groundItemPrefab, position, Quaternion.identity);
            var gi = groundItemObject.GetComponent<GroundItem>();

            var room = parent.GetComponent<Room>();
            
            if (parent != null && room != null)
            {
                room.roomChildObjects.Add(groundItemObject);
            }

            
            gi.CurrentParentIdentity = parent;

            NetworkServer.Spawn(groundItemObject);
            gi.CmdSetItem(item);
            gi.GenerateCost(costMultiplier);

            return gi;
        }

        public static Chest SpawnChest(int rarityBoost, Vector2 position, NetworkIdentity parent) => SpawnChest(rarityBoost, null, position, parent);
        public static Chest SpawnChest(Item presetItem, Vector2 position, NetworkIdentity parent) => SpawnChest(0, presetItem, position, parent);
        
        [Server]
        private static Chest SpawnChest(int rarityBoost, Item presetItem, Vector2 position, NetworkIdentity parent)
        {
            var chestObject = Instantiate(_instance.chestPrefab, position, Quaternion.identity);
            var chest = chestObject.GetComponent<Chest>();

            var room = parent.GetComponent<Room>();
            
            if (parent != null && room != null)
            {
                room.roomChildObjects.Add(chestObject);
            }
            
            chest.CurrentParentIdentity = parent;
            chest.rarityBoost = rarityBoost;
            
            if(presetItem != null) chest.SetItem(presetItem);
            
            NetworkServer.Spawn(chestObject);

            return chest;
        }
    }
}
