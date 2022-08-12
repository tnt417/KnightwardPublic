using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;
using Player = TonyDev.Game.Core.Entities.Player.Player;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        private static EnemySpawnManager _instance;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(this);
        }

        //All enemies should be spawned using this method.
        public static Enemy SpawnEnemy(EnemyData enemyData, Vector2 position, Transform parent)
        {
            var enemy = Instantiate(enemyData.prefab == null ? _instance.enemyPrefab : enemyData.prefab, position,
                Quaternion.identity, parent).GetComponent<Enemy>();
            enemy.SetEnemyData(enemyData);
            return enemy;
        }

        [GameCommand(Keyword = "spawn", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Spawned.")]
        public static void SpawnEnemyCommand(string enemyName, int amount = 1)
        {
            for (var i = 0; i < amount; i++)
                if (Camera.main is not null)
                    SpawnEnemy(ObjectDictionaries.Enemies[enemyName],
                        Camera.main.ScreenToWorldPoint(Input.mousePosition), null);
        }
    }
}