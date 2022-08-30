using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using UnityEngine;

namespace TonyDev.Game.Global
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        public float seconds;
        public GameObject spawnPrefabOnDestroy;
        private float _timer;
        private GameEntity _owner;
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= seconds && seconds > 0)
            {
                Destroy(gameObject);
            }
        }

        private bool _isQuitting = false;
        
        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }
        
        private void OnDestroy()
        {
            if (!_isQuitting && spawnPrefabOnDestroy != null)
            {
                var spawned = Instantiate(spawnPrefabOnDestroy, transform.position, Quaternion.identity);
                var attackComponent = spawned.GetComponent<AttackComponent>();
                if(attackComponent != null) attackComponent.SetData(null, _owner);
            }
        }

        public void SetOwner(GameEntity entity)
        {
            _owner = entity;
        }
    }
}
