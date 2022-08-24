using System.Collections.Generic;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global.Console;
using UnityEngine;

namespace TonyDev.Game.Global
{
    public class ObjectSpawner : MonoBehaviour
    {
        private static ObjectSpawner _instance;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private float moneyOutwardForce;
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private int objectPoolSize;
        private static readonly Queue<GameObject> PopupObjectPool = new ();
        
        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        public static void SpawnPopup(Vector2 position, int damage, bool critical)
        {
            var go = PopupObjectPool.Count >= _instance.objectPoolSize ? PopupObjectPool.Dequeue() : Instantiate(_instance.damageNumberPrefab, position, Quaternion.identity);

            go.transform.position = position;
            
            var tmp = go.GetComponentInChildren<TextMeshPro>();
            var anim = go.GetComponent<Animator>();
            
            anim.Play("DamagePopup");

            tmp.text = "-" + damage;
            tmp.color = critical ? Color.red : Color.white;
            
            PopupObjectPool.Enqueue(go);
        }

        public static void SpawnMoney(int amount, Vector2 originPos)
        {
            for (var i = 0; i < amount; i++)
            {
                var angle = Random.Range(0, 360);

                var radAngle = angle * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
                
                var moneyObject = Instantiate(_instance.moneyPrefab, originPos, Quaternion.identity);

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
            
            enemy.SetEnemyData(enemyData);
            
            enemy.CmdSetParentIdentity(parent);

            return;
        }

        [GameCommand(Keyword = "spawn", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Spawned.")]
        public static void SpawnEnemyCommand(string enemyName, int amount = 1)
        {
            for (var i = 0; i < amount; i++)
                if (Camera.main is not null)
                    GameManager.Instance.CmdSpawnEnemy(enemyName,
                        Camera.main.ScreenToWorldPoint(Input.mousePosition), null);
        }
    }
}
