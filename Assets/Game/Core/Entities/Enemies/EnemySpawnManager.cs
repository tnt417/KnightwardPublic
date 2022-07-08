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
        
        public static void SpawnEnemy(EnemyData enemyData, Vector2 position, Transform parent)
        {
            var enemy = Instantiate(_instance.enemyPrefab, position, Quaternion.identity, parent).GetComponent<Enemy>();
            enemy.SetEnemyData(enemyData);
        }
    }
}
