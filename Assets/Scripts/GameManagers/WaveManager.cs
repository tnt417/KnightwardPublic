using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float range;
    [SerializeField] private float frequency = 0.5f;
    private float _timer;
    void Update()
    {
        _timer += Time.deltaTime;
        
        if (_timer > frequency)
        {
            SpawnEnemy(GetSpawnpoint(), enemyPrefab);
            _timer = 0;
        }
    }

    private void SpawnEnemy(Vector3 position, GameObject prefab)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
    }

    private Vector3 GetSpawnpoint()
    {
        float x = Random.Range(-range, range);
        float y = Random.Range(-range, range);
        return new Vector3(x, y, 0);
    }
}
