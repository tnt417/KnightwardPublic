using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public abstract class Tower : GameEntity
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        [SerializeField] protected float targetRadius;
        [SerializeField] private float fireCooldown;
        [SerializeField] private Team targetTeam = Team.Enemy;
        //

        private float _fireTimer;
        protected Vector2 Direction => (Target.transform.position - transform.position).normalized;

        public void Update()
        {
            if (Target == null) UpdateTarget(); //Get a target if our target is null.

            _fireTimer += Time.deltaTime; //Tick our fire timer
            if (_fireTimer >= fireCooldown && Target != null) Fire(); //Fire when cooldown is over
        }

        private void Fire()
        {
            if (Vector2.Distance(transform.position, Target.transform.position) > targetRadius) UpdateTarget();
            if (Target == null) return;
            _fireTimer = 0;
            OnFire();
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
        protected abstract void TowerUpdate();
        protected abstract void OnFire();
    }
}