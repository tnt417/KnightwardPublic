using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Mirror;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
{
    public class EnemySpawner : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private int baseAllowance;
        [SerializeField] private bool disableAllowanceScaling;
        [SerializeField] private EnemySpawn[] enemySpawns;
        [SerializeField] private float range;
        [SerializeField] private float frequency = 0.5f;

        [SerializeField] public bool autoSpawn;

        //
        private float _timer;
        public bool InRoom => GetComponentInParent<Room>() != null;
        private Room _parentRoom;

        public bool CurrentlySpawning => _startedSpawning &&
                                         _modifiedAllowance - _usedAllowance >=
                                         enemySpawns.Min(es => es.difficultyRating);

        private int _modifiedAllowance;
        private int _usedAllowance;

        private bool _startedSpawning;

        //[SerializeReference] public GameEffectList enemyStartingEffects = new();

        [ServerCallback]
        private void Start()
        {
            _modifiedAllowance = disableAllowanceScaling
                ? baseAllowance
                : (int) (baseAllowance * Mathf.Clamp(GameManager.EnemyDifficultyScale * 0.1f, 1, Mathf.Infinity));
            GameManager.EnemySpawners.Add(this); //Add this spawner to the GameManager's list
            _parentRoom = GetComponentInParent<Room>();

            if (autoSpawn && NetworkServer.active)
            {
                SpawnEnemyTask().Forget();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1.0f, 0.647f, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        [ServerCallback]
        private async UniTask SpawnEnemyTask()
        {
            if (_startedSpawning) return;
            _startedSpawning = true;

            await UniTask.WaitUntil(() => _parentRoom == null || _parentRoom.started);

            while (true)
            {
                var spawns = enemySpawns.Where(es => es.difficultyRating <= _modifiedAllowance - _usedAllowance)
                    .ToList();

                if (spawns.Count == 0) break;

                var enemySpawn = GameTools.SelectRandom(spawns);

                _usedAllowance += enemySpawn.difficultyRating;

                SpawnEnemies(enemySpawn.enemyPrefabs);

                if (frequency > 0) await UniTask.Delay(TimeSpan.FromSeconds(frequency));
            }

            GameManager.EnemySpawners.Remove(this);
            Destroy(this);
        }

        [ServerCallback]
        private void SpawnEnemies(GameObject[] prefabs) //Spawns an enemy at specific position
        {
            if (prefabs == null) return;

            if (_parentRoom != null && !_parentRoom.isServer) return;

            foreach (var prefab in prefabs)
            {
                var enemy = ObjectSpawner.SpawnEnemy(prefab, GetSpawnpoint(), _parentRoom.netIdentity);
                
                // if (enemyStartingEffects is {gameEffects: { }})
                //     enemy.OnStart += () =>
                //     {
                //         foreach (var se in enemyStartingEffects.gameEffects)
                //         {
                //             enemy.CmdAddEffect(se, enemy);
                //         }
                //     };
                //var enemy = ObjectSpawner.SpawnEnemy(enemyData, position, _parentRoom.netIdentity); //Instantiate the enemy

                if (_parentRoom != null)
                {
                    _parentRoom.OnEnemySpawn();
                }
            }
            //
        }

        public void StartSpawn() //Currently called in editor events
        {
            SpawnEnemyTask().Forget();
        }

        private Vector3 GetSpawnpoint() //Get a random spawn point within a certain range
        {
            var x = Random.Range(-range, range);
            var y = Random.Range(-range, range);
            return transform.position + new Vector3(x, y, 0);
        }
    }
}