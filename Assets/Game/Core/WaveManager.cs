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
        //Editor variables
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius;
        [SerializeField] private EnemySpawn[] enemySpawns;
        [SerializeField] private float waveCooldown;
        //
        
        private static float DifficultyThreshold => 1000 * (1 + GameManager.EnemyDifficultyScale/2);
        private float _waveTimer = 0;
        public int wavesSpawned = 0;
        public int TimeUntilNextWaveSeconds => (int)(waveCooldown - _waveTimer);

        private void Update()
        {
            waveCooldown = wavesSpawned % 5 == 0 ? 120 : 30; //Every 5 waves, have a 2 minute break. Otherwise 30 seconds in between waves.
            
            _waveTimer += Time.deltaTime; //Tick the wave timer
            if (_waveTimer >= waveCooldown) SpawnWave(); //Spawn a wave if cooldown is over
        }

        //Spawns a wave of enemies
        private void SpawnWave()
        {
            /* DESCRIPTION:
             * Chooses random spawns of enemies to spawn in random locations until it runs out of difficulty allowance.
             */

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

        //Instantiates enemies from a group of prefabs
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
