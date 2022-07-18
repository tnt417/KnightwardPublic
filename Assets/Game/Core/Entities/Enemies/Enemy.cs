using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Combat;
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
    [RequireComponent(typeof(EnemyAnimator), typeof(CircleCollider2D))]
    public class Enemy : GameEntity
    {
        #region Variables
        [SerializeField] private EnemyAnimator enemyAnimator;
        private int MoneyReward => _enemyData.baseMoneyReward;
        public override Team Team => Team.Enemy;
        public override float DamageMultiplier => Mathf.Clamp01(1f - (Mathf.Log10(GameManager.EnemyDifficultyScale) - 0.5f)); //Enemies essentially gain damage resist as the difficulty scales.
        public override int MaxHealth => _enemyData.maxHealth;
        private EnemyData _enemyData = null;
        private EnemyMovementBase _enemyMovementBase;
        public delegate void AttackAction();
        public event AttackAction OnAttack;
        private float AttackTimerMax => _enemyData.attackData.attackCooldown;
        private float _attackTimer;
        private List<EnemyShootProjectile> _shootProjectiles;
        #endregion

        //Set enemy data, called on spawn
        public void SetEnemyData(EnemyData enemyData)
        {
            if (_enemyData != null) return;
            
            _enemyData = enemyData;

            var coll = GetComponent<CircleCollider2D>();
            
            if(coll != null) coll.radius = _enemyData.hitboxRadius;
            
            enemyAnimator.Set(enemyData);
            CreateMovementComponent(enemyData.movementData);
            CreateAttackObjects(enemyData.attackData);
        }

        //Tick attack timers
        private void Update()
        {
            if (AttackTimerMax < 0) return;
            
            _attackTimer += Time.deltaTime;
            
            if (_attackTimer >= AttackTimerMax)
            {
                Attack();
                _attackTimer = 0;
            }
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

        //Add attack objects based on attack data
        private void CreateAttackObjects(EnemyAttackData attackData)
        {
            var parentTransform = transform;
            
            var attackObject = new GameObject("Attack Object");
            attackObject.transform.parent = parentTransform;
            attackObject.transform.localPosition = new Vector3(0, 0, 0);
            
            switch (attackData)
            {
                case EnemyAttackContactData contactData:
                    var coll = attackObject.AddComponent<CircleCollider2D>();
                    var dmg = attackObject.AddComponent<DamageComponent>();
                    coll.isTrigger = true;
                    coll.radius = contactData.hitboxRadius;
                    dmg.damage = contactData.damage;
                    dmg.team = contactData.team;
                    dmg.damageCooldown = contactData.DamageCooldown;
                    dmg.destroyOnApply = contactData.destroyOnApply;
                    dmg.knockbackMultiplier = contactData.knockbackMultiplier;
                    break;
                case EnemyAttackProjectileData projData:
                    foreach (var data in projData.projectileDatas)
                    {
                        _shootProjectiles = new List<EnemyShootProjectile>();
                        
                        var proj = attackObject.AddComponent<EnemyShootProjectile>();
                        proj.Set(data, this);
                        
                        _shootProjectiles.Add(proj);
                    }
                    break;
            }
        }

        //Initialize variables and events
        private void Start()
        {
            Init();
            
            //Initialize variables
            enemyAnimator = GetComponent<EnemyAnimator>();
            //
            
            SubscribeAnimatorEvents();
            
            OnDeath += EnemyDie;
        }

        //Sets up the animator to play certain animations on certain events
        private void SubscribeAnimatorEvents()
        {
            OnDeath += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Die);
            OnHurt += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Hurt);
            _enemyMovementBase.OnStartMove += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Move);
            _enemyMovementBase.OnStopMove += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Stop);
            OnAttack += () => enemyAnimator.PlayAnimation(EnemyAnimationState.Attack);
        }

        //Invokes the attack event
        public void Attack() //Called either on a timer or through animation events.
        {
            OnAttack?.Invoke();
        }

        //Give money reward and destroy self.
        private void EnemyDie()
        {
            GameManager.Money += MoneyReward;
            Destroy(gameObject);
        }
    }
}
