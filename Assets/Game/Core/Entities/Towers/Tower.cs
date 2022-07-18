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
        //

        protected float AttackSpeedMultiplier = 1;
        private float _fireTimer;

        public void Update()
        {
            if (Targets.Count == 0) UpdateTarget(); //Get a target if our target is null.

            _fireTimer += Time.deltaTime; //Tick our fire timer
            if (_fireTimer * AttackSpeedMultiplier >= fireCooldown)
                Fire(); //Fire when cooldown is over
        }

        private void Fire()
        {
            UpdateTarget();
            _fireTimer = 0;
            if (Targets.Count == 0) return;
            OnFire();
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
        protected abstract void TowerUpdate();
        protected abstract void OnFire();
    }
}