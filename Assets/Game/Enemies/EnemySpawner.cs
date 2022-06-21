using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    //Editor variables
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float range;
    [SerializeField] private float frequency = 0.5f;
    [SerializeField] private int destroyAfterSpawns;
    //
    
    private int _spawns;
    private float _timer;

    private void Awake()
    {
        GameManager.EnemySpawners.Add(this); //Add this spawner to the GameManager's list
    }
    
    private void Start()
    {
        SpawnEnemy(GetSpawnpoint(), enemyPrefab); //Spawn 1 copy of our enemy on start
    }

    void Update()
    {
        _timer += Time.deltaTime; //Tick the timer
        
        if (_timer > frequency)
        {
            SpawnEnemy(GetSpawnpoint(), enemyPrefab); //Spawn an enemy after enough time has elapsed
            _timer = 0; //And reset the timer
        }
    }

    private void SpawnEnemy(Vector3 position, GameObject prefab) //Spawns an enemy at specific position
    {
        if (destroyAfterSpawns == 0)
        {
            GameManager.EnemySpawners.Remove(this); //Don't spawn the enemy if 0 things are supposed to be spawned
            Destroy(this); //And destroy the script
            return;
        }
        var go = Instantiate(prefab, position, Quaternion.identity, transform); //Instantiate the enemy
        _spawns++; //Increase the spawn count
        
        //Destroy the script if enough enemies have been spawned
        if (_spawns >= destroyAfterSpawns)
        {
            GameManager.EnemySpawners.Remove(this);
            Destroy(this);
        }
        //
    }

    private Vector3 GetSpawnpoint() //Get a random spawn point within a certain range
    {
        var x = Random.Range(-range, range);
        var y = Random.Range(-range, range);
        return transform.position + new Vector3(x, y, 0);
    }
}
