using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
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

        [SyncVar(hook=nameof(EnemyDataHook))] private EnemyData _enemyData;

        [SerializeField] private EnemyAnimator enemyAnimator;
        private EnemyMovementBase _enemyMovementBase;
        private int MoneyReward => _enemyData.baseMoneyReward;
        public override Team Team => Team.Enemy;
        protected override float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);

        #endregion

        private void EnemyDataHook(EnemyData oldData, EnemyData newData)
        {
            if (oldData == null && newData != null)
            {
                SetEnemyData(newData);
            }
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSetEnemyData(EnemyData enemyData)
        {
            _enemyData = enemyData;
        }

        //Set enemy data, called on every client on spawn
        private void SetEnemyData(EnemyData enemyData)
        {
            _enemyData = enemyData;
            
            baseStats = enemyData.baseStats;

            var hitbox = GetComponent<CircleCollider2D>();
            var coll = GetComponent<BoxCollider2D>();

            if (hitbox != null) hitbox.radius = _enemyData.hitboxRadius;
            if (coll != null) coll.size = _enemyData.hitboxRadius * Vector2.one;

            enemyAnimator.Set(enemyData);
            if(EntityOwnership) CreateMovementComponent(enemyData.movementData);
            SubscribeAnimatorEvents();
            
            InitProjectiles(enemyData.projectileAttackData);
            InitContactDamage(enemyData.contactAttackData);

            Init();
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
                        GameManager.Instance.CmdSpawnProjectile(netIdentity, transform.position, direction, data, AttackComponent.GetUniqueIdentifier(this));
                };
            }
        }

        private void InitContactDamage(AttackData contactAttackData)
        {
            AttackFactory.CreateStaticAttack(this, contactAttackData, true, null);
        }
        
        //Initialize variables and events
        private void Start()
        {
            //Initialize variables
            enemyAnimator = GetComponent<EnemyAnimator>();
            //

            if(isServer) OnDeath += (value) => EnemyDie();
        }

        //Sets up the animator to play certain animations on certain events
        private void SubscribeAnimatorEvents()
        {
            OnLocalHurt += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Hurt);
            if (!EntityOwnership) return;
            OnDeath += (float value) => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Die);
            OnAttack += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Attack);
            _enemyMovementBase.OnStartMove += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Move);
            _enemyMovementBase.OnStopMove += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Stop);
        }

        //Give money reward and destroy self.
        private void EnemyDie()
        {
            GameManager.Instance.CmdSpawnMoney(MoneyReward, transform.position, CurrentParentIdentity);
        }
    }
}