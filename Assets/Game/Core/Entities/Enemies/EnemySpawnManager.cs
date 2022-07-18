using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using UnityEngine;

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
        
        public static Enemy SpawnEnemy(EnemyData enemyData, Vector2 position, Transform parent)
        {
            var enemy = Instantiate(enemyData.prefab == null ? _instance.enemyPrefab : enemyData.prefab, position, Quaternion.identity, parent).GetComponent<Enemy>();
            enemy.SetEnemyData(enemyData);
            return enemy;
        }
    }
}
