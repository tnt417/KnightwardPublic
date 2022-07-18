using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Movement
{
    [RequireComponent(typeof(Enemy), typeof(EnemyAnimator))]
    public abstract class EnemyMovementBase : MonoBehaviour, IMovement
    {
        private Enemy _enemy;
        protected EnemyMovementData EnemyMovementData;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (FirstTarget == null) return;
            
            UpdateMovement(); //Call UpdateMovement
            if(EnemyMovementData.doXFlipping) _spriteRenderer.flipX = FirstTarget.position.x > transform.position.x;
        }

        public bool DoMovement => true;
        public abstract void UpdateMovement();
        public float SpeedMultiplier { get; set; } = 1;
        protected Transform FirstTarget => _enemy.Targets.First();

        public delegate void MoveAction();

        public abstract void PopulateFromData(EnemyMovementData data);

        public IEnumerator ModifySpeedForSeconds(float multiplier, float seconds)
        {
            SpeedMultiplier = multiplier;
            yield return new WaitForSeconds(seconds);
            SpeedMultiplier = 1;
        }

        public abstract event MoveAction OnStartMove;
        public abstract event MoveAction OnStopMove;
    }
}
