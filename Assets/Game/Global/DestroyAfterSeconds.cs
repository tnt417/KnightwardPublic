using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        public float seconds;
        public GameObject spawnPrefabOnDestroy;
        private float _timer;
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= seconds)
            {
                Destroy(gameObject);
            }
        }

        private bool isQuitting = false;
        
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }
        
        private void OnDestroy()
        {
            if(!isQuitting && spawnPrefabOnDestroy != null)
                Instantiate(spawnPrefabOnDestroy, transform.position, Quaternion.identity);
        }
    }
}
