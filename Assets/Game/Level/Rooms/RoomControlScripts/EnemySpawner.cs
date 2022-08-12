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
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private float range;
        [SerializeField] private float frequency = 0.5f;
        [SerializeField] public int destroyAfterSpawns;
        [SerializeField] public bool autoSpawn;
        //
        public int Spawns { get; private set; }
        private float _timer;
        public bool InRoom => GetComponentInParent<Room>() != null;
        private Room _parentRoom;
        public bool CurrentlySpawning => autoSpawn && Spawns < destroyAfterSpawns;

        private void Awake()
        {
            GameManager.EnemySpawners.Add(this); //Add this spawner to the GameManager's list
            _parentRoom = GetComponentInParent<Room>();
        }
    
        private void Start()
        {
            if(autoSpawn) SpawnEnemy(GetSpawnpoint(), enemyData); //Spawn 1 copy of our enemy on start
        }

        private void Update()
        {
            if (!autoSpawn) return;
            
            _timer += Time.deltaTime; //Tick the timer
        
            if (_timer > frequency)
            {
                SpawnEnemy(GetSpawnpoint(), enemyData); //Spawn an enemy after enough time has elapsed
                _timer = 0; //And reset the timer
            }
        }

        private void SpawnEnemy(Vector3 position, EnemyData data) //Spawns an enemy at specific position
        {
            if (destroyAfterSpawns == 0)
            {
                GameManager.EnemySpawners.Remove(this); //Don't spawn the enemy if 0 things are supposed to be spawned
                Destroy(this); //And destroy the script
                return;
            }
            
            var enemy = EnemySpawnManager.SpawnEnemy(data, position, transform); //Instantiate the enemy
            if (_parentRoom != null)
            {
                enemy.OnDeath += _parentRoom.OnEnemyDie;
                _parentRoom.OnEnemySpawn();
            }
            Spawns++; //Increase the spawn count
        
            //Destroy the script if enough enemies have been spawned
            if (Spawns >= destroyAfterSpawns)
            {
                GameManager.EnemySpawners.Remove(this);
                Destroy(this);
            }
            //
        }

        public void StartSpawn() //Currently called in editor events
        {
            autoSpawn = true;
        }

        private Vector3 GetSpawnpoint() //Get a random spawn point within a certain range
        {
            var x = Random.Range(-range, range);
            var y = Random.Range(-range, range);
            return transform.position + new Vector3(x, y, 0);
        }
    }
}
