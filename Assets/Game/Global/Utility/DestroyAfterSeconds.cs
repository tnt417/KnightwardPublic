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
                var attackComponents = spawned.GetComponentsInChildren<AttackComponent>();

                foreach (var att in attackComponents)
                {
                    att.SetData(null, _owner);
                    att.OnDamageDealt += (d, ge, crit, dt) => _owner.OnDamageOther?.Invoke(d, ge, crit, dt);
                }
                var destroy = spawned.GetComponent<DestroyAfterSeconds>();
                if(destroy != null) destroy.SetOwner(_owner);
            }
        }

        public void SetOwner(GameEntity entity)
        {
            _owner = entity;
        }
    }
}
