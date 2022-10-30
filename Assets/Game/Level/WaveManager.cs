using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level
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

        public float breakLength;
        public float breakFrequency;
        public int regularLength;
        [Range(0f,1f)]
        public float percentageActiveSpawning;
        //

        private float _waveCooldown;

        private static float DifficultyThreshold => 1000f * (1f + GameManager.EnemyDifficultyScale / 1.5f) *
                                                    (0.55f + 0.45f * NetworkServer.connections.Count);

        private float _waveTimer = 0;
        public int wavesSpawned = 0;
        public int TimeUntilNextWaveSeconds => (int) (_waveCooldown - _waveTimer);

        private void Update()
        {
            _waveCooldown =
                wavesSpawned % breakFrequency == 0
                    ? breakLength
                    : regularLength;

            _waveTimer += Time.deltaTime * Timer.TickSpeedMultiplier; //Tick the wave timer
            
            if (_waveTimer >= _waveCooldown)
            {
                _waveTimer = 0;
                wavesSpawned++;
                SpawnWave().Forget(); //Spawn a wave if cooldown is over
            }
        }

        //Spawns a wave of enemies
        [GameCommand(Keyword = "nextwave", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Spawning next wave.")]
        [ServerCallback]
        public async UniTask SpawnWave()
        {
            /* DESCRIPTION:
             * Chooses random spawns of enemies to spawn in random locations until it runs out of difficulty allowance.
             */

            float difficultyTotal = 0;

            List<EnemySpawn> spawns = new();
            
            while (difficultyTotal <= DifficultyThreshold)
            {
                var enemySpawn = enemySpawns[Random.Range(0, enemySpawns.Length)];
                if (enemySpawn.difficultyRating > DifficultyThreshold - difficultyTotal) continue;
                spawns.Add(enemySpawn);
                difficultyTotal += enemySpawn.difficultyRating;
                if (!enemySpawns.Contains(
                    enemySpawns //If enemySpawns doesn't have any spawns that could use up the remaining difficulty allowance, we're done spawning.
                        .FirstOrDefault(es => es.difficultyRating <= DifficultyThreshold - difficultyTotal))) break;
            }

            var spawnCount = spawns.Count;
            var spawningPeriod = percentageActiveSpawning * regularLength;
            
            while (spawns.Count > 0)
            {
                var spawn = spawns[0];
                SpawnEnemies(spawn.enemyPrefabs);
                spawns.Remove(spawn);
                await UniTask.Delay(TimeSpan.FromSeconds(spawningPeriod/spawnCount));
            }
            
            if (wavesSpawned % 5 == 0)
            {
                Crystal.Instance.CmdDamageEntity(-(Crystal.Instance.MaxHealth * 0.33f), false, null, true);
            }
        }

        //Instantiates enemies from a group of prefabs
        [ServerCallback]
        private void SpawnEnemies(IEnumerable<GameObject> enemyPrefabs)
        {
            foreach (var e in enemyPrefabs)
            {
                GameManager.Instance.CmdSpawnEnemy(ObjectFinder.GetNameOfPrefab(e),
                    spawnPoints[Random.Range(0, spawnPoints.Length)].position
                    + (Vector3) new Vector2(Random.Range(-spawnRadius, spawnRadius),
                        Random.Range(-spawnRadius, spawnRadius)), null);
            }
        }
    }
}