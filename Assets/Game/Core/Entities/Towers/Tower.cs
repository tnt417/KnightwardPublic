using System.Linq;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public abstract class Tower : MonoBehaviour
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        [SerializeField] protected float targetRadius;
        [SerializeField] private float fireCooldown;
        //

        private float _fireTimer;
        private Transform _target;
        protected Vector2 Direction => (_target.transform.position - transform.position).normalized;

        public void Update()
        {
            if(_target == null) GetTarget(); //Get a target if our target is null.

            _fireTimer += Time.deltaTime; //Tick our fire timer
            if (_fireTimer >= fireCooldown && _target != null) Fire(); //Fire when cooldown is over
        }

        private void Fire()
        {
            if (Vector2.Distance(transform.position, _target.transform.position) > targetRadius) GetTarget(); 
            if (_target == null) return;
            _fireTimer = 0;
            OnFire();
        }

        protected abstract void TowerUpdate();
        protected abstract void OnFire();
        private void GetTarget() //Set the nearest enemy in range as target
        {
            var enemies = GameManager.Enemies;
            _target = enemies
                .OrderBy(e => Vector2.Distance(e.transform.position, transform.position))
                .FirstOrDefault(e => Vector2.Distance(e.transform.position, transform.position) < targetRadius)?.transform;
        }
    }
}
