using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level
{
    [Serializable]
    public struct EnemySpawn
    {
        public GameObject[] enemyPrefabs;
        public int difficultyRating;
        public int minFloor;
    }

    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;

        public Action<int> OnWaveBegin;
        public Action<int> OnWaveEnd;

        //Editor variables
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius;

        [SerializeField] private EnemySpawn[] enemySpawns;

        public float breakLength = 30f;
        public float breakFrequency;
        public int regularLength;

        [Range(0f, 1f)] public float percentageActiveSpawning;
        //

        private float _waveCooldown;

        private static float DifficultyThreshold =>
            500f * (1f + Mathf.Pow(GameManager.EnemyDifficultyScale, 1.15f) / 1.5f) *
            (0.8f + 0.2f * NetworkServer.connections.Count);

        private float _waveTimer = 0;
        public int wavesSpawned = 0;

        private bool OnBreak => wavesSpawned > 0 && wavesSpawned % breakFrequency == 0;

        private bool _paused;

        [GameCommand(Keyword = "pausewave", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Toggled spawning.")]
        public static void Pause()
        {
            Instance._paused = !Instance._paused;
        }

        private void Awake()
        {
            Instance = this;
        }

        [ServerCallback]
        public void MoveEnemyToWave(Enemy enemy)
        {
            enemy.CmdSetParentIdentity(null);
            enemy.transform.position = GetSpawnpoint();
        }

        private void Start()
        {
            _waveCooldown = OnBreak
                ? breakLength
                : regularLength;
        }

        [ServerCallback]
        private void Update()
        {
            if (GameManager.Instance == null || _paused) return;

            _waveTimer += Time.deltaTime * Timer.TickSpeedMultiplier; // * BreakPassingMultiplier; //Tick the wave timer

            if (NetworkServer.active) GameManager.Instance.CmdSetWaveProgress(wavesSpawned, _waveTimer / _waveCooldown);

            if (_waveTimer >= _waveCooldown)
            {
                NextWave(); //Spawn a wave if cooldown is over
            }

            if (!OnBreak && !GameManager.EntitiesReadonly.Any(e => e.CurrentParentIdentity == null && e is Enemy))
            {
                NextWave(); //When we clear a wave spawn the next one to prevent boredom
            }
        }

        [ServerCallback]
        public void StallTime(float seconds)
        {
            _waveCooldown += seconds;
        }

        public static float BreakPassingMultiplier => 1f;

        [GameCommand(Keyword = "nextwave", PermissionLevel = PermissionLevel.Cheat,
            SuccessMessage = "Spawning next wave.")]
        [ServerCallback]
        public void NextWave()
        {
            _waveTimer = 0;
            wavesSpawned++;

            _waveCooldown = OnBreak
                ? breakLength
                : regularLength;
            
            OnWaveBegin?.Invoke(wavesSpawned);

            SpawnWave().Forget();
        }

        [SerializeField] private float bigWaveDifficultyMult;

        [SerializeField] private int bigWaveFrequency;

        public bool OnBigWave => wavesSpawned % bigWaveFrequency == 0 && wavesSpawned > 0;
        
        //Spawns a wave of enemies
        [ServerCallback]
        public async UniTask SpawnWave()
        {
            /* DESCRIPTION:
             * Chooses random spawns of enemies to spawn in random locations until it runs out of difficulty allowance.
             */

            var threshold = OnBigWave
                ? bigWaveDifficultyMult * DifficultyThreshold
                : DifficultyThreshold;

            float difficultyTotal = 0;

            List<EnemySpawn> spawns = new();

            while (difficultyTotal <= threshold)
            {
                var enemySpawn = enemySpawns[Random.Range(0, enemySpawns.Length)];
                if (enemySpawn.difficultyRating > threshold - difficultyTotal || enemySpawn.minFloor > GameManager.DungeonFloor) continue;
                spawns.Add(enemySpawn);
                difficultyTotal += enemySpawn.difficultyRating;
                if (!enemySpawns.Contains(
                    enemySpawns //If enemySpawns doesn't have any spawns that could use up the remaining difficulty allowance, we're done spawning.
                        .FirstOrDefault(es => es.difficultyRating <= threshold - difficultyTotal))) break;
            }

            var spawnCount = spawns.Count;
            var spawningPeriod = percentageActiveSpawning * regularLength;

            while (spawns.Count > 0)
            {
                var spawn = spawns[0];
                SpawnEnemies(spawn.enemyPrefabs);
                spawns.Remove(spawn);
                await UniTask.Delay(TimeSpan.FromSeconds(spawningPeriod / spawnCount));
            }
            
            OnWaveEnd?.Invoke(wavesSpawned);

            // if (wavesSpawned % 5 == 0)
            // {
            //     Crystal.Instance.CmdDamageEntity(-(Crystal.Instance.MaxHealth * 0.33f), false, null, true);
            // }
        }

        //Instantiates enemies from a group of prefabs
        [ServerCallback]
        private void SpawnEnemies(IEnumerable<GameObject> enemyPrefabs)
        {
            foreach (var e in enemyPrefabs)
            {
                GameManager.Instance.CmdSpawnEnemy(ObjectFinder.GetNameOfPrefab(e), GetSpawnpoint(), null, 1);
            }
        }

        private Vector2 GetSpawnpoint()
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position
                   + (Vector3) new Vector2(Random.Range(-spawnRadius, spawnRadius),
                       Random.Range(-spawnRadius, spawnRadius));
        }
    }
}