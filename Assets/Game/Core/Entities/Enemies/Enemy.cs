using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Enemies.Movement;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Enemy : GameEntity
    {
        #region Variables

        private EnemyData _enemyData = null;

        [SerializeField] private EnemyAnimator enemyAnimator;
        private EnemyMovementBase _enemyMovementBase;
        private int MoneyReward => _enemyData.baseMoneyReward;
        public override Team Team => Team.Enemy;
        protected override float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);

        #endregion

        //Set enemy data, called on spawn
        public void SetEnemyData(EnemyData enemyData)
        {
            if (_enemyData != null) return;

            _enemyData = enemyData;

            foreach (var sb in enemyData.baseStats)
            {
                Stats.AddStatBonus(sb.statType, sb.stat, sb.strength, sb.source);
            }

            var hitbox = GetComponent<CircleCollider2D>();
            var coll = GetComponent<BoxCollider2D>();

            if (hitbox != null) hitbox.radius = _enemyData.hitboxRadius;
            if (coll != null) coll.size = _enemyData.hitboxRadius * Vector2.one;

            enemyAnimator.Set(enemyData);
            CreateMovementComponent(enemyData.movementData);
            
            InitProjectiles(enemyData.projectileAttackData);
            InitContactDamage(enemyData.contactAttackData);
        }

        //Add movement components based on movement data
        private void CreateMovementComponent(EnemyMovementData movementData)
        {
            var type = movementData switch
            {
                EnemyMovementChaseData => typeof(EnemyMovementChase),
                EnemyMovementStrafeData => typeof(EnemyMovementPeriodicalStrafe),
                _ => typeof(EnemyMovementBase)
            };

            var c = gameObject.AddComponent(type);

            if (c is EnemyMovementBase moveBase)
            {
                moveBase.PopulateFromData(movementData);
                _enemyMovementBase = moveBase;
            }
        }

        private void InitProjectiles(IEnumerable<ProjectileData> projectileData)
        {
            foreach (var data in projectileData)
            {
                OnAttack += () =>
                {
                    foreach (var direction in Targets.Select(
                        t => (t.transform.position - transform.position).normalized))
                        AttackFactory.CreateProjectileAttack(this, direction, data);
                };
            }
        }

        private void InitContactDamage(AttackData contactAttackData)
        {
            AttackFactory.CreateStaticAttack(this, contactAttackData, true, null);
        }
        
        //Initialize variables and events
        private new void Start()
        {
            base.Start();

            //Initialize variables
            enemyAnimator = GetComponent<EnemyAnimator>();
            //

            SubscribeAnimatorEvents();

            OnDeath += EnemyDie;
        }

        //Sets up the animator to play certain animations on certain events
        private void SubscribeAnimatorEvents()
        {
            OnDeath += (float value) => enemyAnimator.PlayAnimation(EnemyAnimationState.Die);
            OnHurt += (float value) => enemyAnimator.PlayAnimation(EnemyAnimationState.Hurt);
            OnAttack += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Attack);
            _enemyMovementBase.OnStartMove += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Move);
            _enemyMovementBase.OnStopMove += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Stop);
        }

        //Give money reward and destroy self.
        private void EnemyDie(float value)
        {
            PickupSpawner.SpawnMoney(MoneyReward, transform.position);
            Destroy(gameObject);
        }
    }
}