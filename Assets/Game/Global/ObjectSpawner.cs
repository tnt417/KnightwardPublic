using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Core.Items.Money;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations;
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
        [SerializeField] private int objectPoolSize;
        private static readonly Queue<GameObject> PopupObjectPool = new();

        private void Awake()
        {
            if (_instance == null) _instance = this;
            
            PopupObjectPool.Clear();
        }

        public static GameObject SpawnProjectile(GameEntity owner, Vector2 pos, Vector2 direction,
            ProjectileData projectileData, bool localOnly = false) =>
            GameManager.Instance.SpawnProjectile(owner, pos, direction.normalized, projectileData, localOnly);

        public static void SpawnDmgPopup(Vector2 position, float damage, bool critical)
        {
            var roundedDamage = Mathf.CeilToInt(damage);

            var go = PopupObjectPool.Count >= _instance.objectPoolSize
                ? PopupObjectPool.Dequeue()
                : Instantiate(_instance.damageNumberPrefab, position, Quaternion.identity);

            go.transform.position = position;

            var tmp = go.GetComponentInChildren<TextMeshPro>();
            var anim = go.GetComponent<Animator>();

            anim.Play("Popup");

            tmp.text = damage == 0
                ? "Dodge"
                : (-roundedDamage).ToString(); //Negative, so positive damage has a '-' and healing doesn't.

            //Yellow = normal damage
            //Red = crit damage
            //Green = normal healing
            //Magenta = crit healing
            tmp.color = damage == 0 ? Color.gray :
                damage > 0 ? critical ? Color.red : Color.yellow :
                critical ? Color.magenta : Color.green;

            PopupObjectPool.Enqueue(go);
        }

        public static void SpawnTower(Item towerItem, Vector2 pos, NetworkIdentity parent)
        {
            var prefab = towerItem.SpawnablePrefab;

            var go = Instantiate(prefab, pos, Quaternion.identity);

            NetworkServer.Spawn(go, NetworkServer.localConnection);

            var tower = go.GetComponent<Tower>();

            tower.CmdSetParentIdentity(parent);
            tower.CmdSetTowerItem(towerItem);
        }

        public static void SpawnTextPopup(Vector2 position, string text, Color color, float speedMultiplier = 1f)
        {
            var go = PopupObjectPool.Count >= _instance.objectPoolSize
                ? PopupObjectPool.Dequeue()
                : Instantiate(_instance.damageNumberPrefab, position, Quaternion.identity);

            go.transform.position = position;

            var tmp = go.GetComponentInChildren<TextMeshPro>();
            var anim = go.GetComponent<Animator>();

            anim.speed = speedMultiplier;
            anim.Play("Popup");

            tmp.text = text;
            tmp.color = color;

            PopupObjectPool.Enqueue(go);
        }

        public static void SpawnMoney(int amount, Vector2 originPos, NetworkIdentity parentRoom,
            bool ignoreModifiers = false)
        {
            var modifier = ignoreModifiers ? 1.0f : 1 + GameManager.MoneyDropBonusFactor;

            for (var i = 0; i < amount * modifier; i += 0)
            {
                var remMoney = amount - i;
                var addAmount = remMoney / 5f > 1 ? remMoney / 25f > 1 ? remMoney / 100f > 1 ? 100 : 25 : 5 : 1;
                
                i += addAmount;

                MoneyObject.SpawnMoney(_instance.moneyPrefab, originPos, addAmount, parentRoom);
            }
        }

        //All enemies should be spawned using this method.
        [Server]
        public static Enemy SpawnEnemy(GameObject prefab, Vector2 position, NetworkIdentity parent)
        {
            if (prefab == null) return null;

            var enemy = Instantiate(prefab, position,
                Quaternion.identity).GetComponent<Enemy>();

            enemy.CurrentParentIdentity = parent;

            NetworkServer.Spawn(enemy.gameObject);

            enemy.netIdentity.AssignClientAuthority(NetworkServer.localConnection);

            enemy.CmdSetParentIdentity(parent);

            return enemy;
        }

        [GameCommand(Keyword = "spawn", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Spawned.")]
        public static void SpawnEnemyCommand(string enemyName, int amount = 1)
        {
            if (Camera.main is not null)
                GameManager.Instance.CmdSpawnEnemy(enemyName,
                    Camera.main.ScreenToWorldPoint(Input.mousePosition),
                    Player.LocalInstance.CurrentParentIdentity, amount);
        }

        [GameCommand(Keyword = "spawnallitems", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Spawned.")]
        public static void SpawnAllItems()
        {
            var items = GameManager.AllItems.ToArray();

            var sqrLength = (int) Mathf.Sqrt(items.Length);

            for (var i = 0; i < items.Length; i++)
            {
                var x = i / sqrLength;
                var y = i % sqrLength;

                var item = ItemGenerator.GenerateItemFromData(items[i]);

                GameManager.Instance.CmdSpawnItem(item, 0,
                    (Vector2) Player.LocalInstance.transform.position +
                    new Vector2((x - sqrLength / 2) * 2, (y - sqrLength / 2) * 2),
                    Player.LocalInstance.CurrentParentIdentity);
            }
        }

        [Server]
        public static GroundItem SpawnGroundItem(Item item, float costMultiplier, Vector2 position,
            NetworkIdentity parent)
        {
            var groundItemObject = Instantiate(_instance.groundItemPrefab, position, Quaternion.identity);
            var gi = groundItemObject.GetComponent<GroundItem>();

            if (gi == null)
            {
                Debug.LogWarning("Something went wrong while spawning ground item!");
                return null;
            }

            if (parent != null)
            {
                var room = RoomManager.Instance.GetRoomFromID(parent.netId);

                if (room != null)
                {
                    room.roomChildObjects.Add(groundItemObject);
                }
            }

            gi.CurrentParentIdentity = parent;

            NetworkServer.Spawn(groundItemObject);
            gi.CmdSetItem(item);
            gi.CmdSetCost((int) (ItemGenerator.GenerateCost(item, GameManager.DungeonFloor) * costMultiplier));
            gi.CmdSetEssence(ItemGenerator.GenerateEssence(item));

            return gi;
        }

        public static Chest SpawnChest(int rarityBoost, Vector2 position, NetworkIdentity parent) =>
            SpawnChest(rarityBoost, null, position, parent);

        public static Chest SpawnChest(Item presetItem, Vector2 position, NetworkIdentity parent) =>
            SpawnChest(0, presetItem, position, parent);

        [Server]
        private static Chest SpawnChest(int rarityBoost, Item presetItem, Vector2 position, NetworkIdentity parent)
        {
            var chestObject = Instantiate(_instance.chestPrefab, position, Quaternion.identity);
            var chest = chestObject.GetComponent<Chest>();

            if (parent != null)
            {
                var room = RoomManager.Instance.GetRoomFromID(parent.netId);

                if (room != null)
                {
                    room.roomChildObjects.Add(chestObject);
                }
            }

            chest.CurrentParentIdentity = parent;
            chest.rarityBoost = rarityBoost;

            if (presetItem != null) chest.SetItem(presetItem);

            NetworkServer.Spawn(chestObject);

            return chest;
        }
    }
}