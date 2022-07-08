using System;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Movement
{
    [RequireComponent(typeof(Enemy), typeof(EnemyAnimator))]
    public abstract class EnemyMovementBase : MonoBehaviour, IMovement
    {
        private Enemy _enemy;
        protected EnemyMovementData EnemyMovementData;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void FixedUpdate()
        {
            if(Target != null) UpdateMovement(); //Call UpdateMovement
        }

        public bool DoMovement => true;
        public abstract void UpdateMovement();
        public float SpeedMultiplier { get; set; } = 1;
        protected Transform Target => _enemy.Target;

        public delegate void MoveAction();

        public abstract void PopulateFromData(EnemyMovementData data);

        public abstract event MoveAction OnStartMove;
        public abstract event MoveAction OnStopMove;
    }
}
