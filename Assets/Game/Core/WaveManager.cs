using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core
{
    [Serializable]
    public struct EnemySpawn
    {
        public GameObject[] enemyPrefabs;
        public float difficultyRating;
    }
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;
        
        //Editor variables
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius;
        [SerializeField] private EnemySpawn[] enemySpawns;
        [SerializeField] private float waveCooldown;
        //
        
        private float DifficultyThreshold => 1000 * (1 + GameManager.EnemyDifficultyScale/2);
        private float _waveTimer = 10000000;
        public float wavesSpawned = 0;

        private void Awake()
        {
            //Singleton code
            if (Instance == null) Instance = this;
            else Destroy(this);
            //
        }
        
        private void Update()
        {
            _waveTimer += Time.deltaTime;
            if (_waveTimer >= waveCooldown) SpawnWave();
        }

        private void SpawnWave()
        {
            Debug.Log(DifficultyThreshold);
            float difficultyTotal = 0;
            
            while (difficultyTotal <= DifficultyThreshold)
            {
                var enemySpawn = enemySpawns[Random.Range(0, enemySpawns.Length)];
                if (enemySpawn.difficultyRating > DifficultyThreshold - difficultyTotal) continue;
                difficultyTotal += enemySpawn.difficultyRating;
                SpawnEnemies(enemySpawn.enemyPrefabs);
                if (!enemySpawns.Contains(enemySpawns //If enemySpawns doesn't have any spawns that could use up the remaining difficulty allowance, we're done spawning.
                    .FirstOrDefault(es => es.difficultyRating <= DifficultyThreshold - difficultyTotal))) break;
            }

            _waveTimer = 0;
            wavesSpawned++;
        }

        private void SpawnEnemies(IEnumerable<GameObject> enemyPrefabs)
        {
            foreach (var e in enemyPrefabs)
            {
                var go = Instantiate(e, spawnPoints[Random.Range(0, spawnPoints.Length)].position
                                        + (Vector3)new Vector2(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius)), Quaternion.identity);
            }
        }


    }
}
