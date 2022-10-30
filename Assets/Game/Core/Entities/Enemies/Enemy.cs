using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies.Modifiers;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Enemy : GameEntity
    {
        #region Variables

        [SerializeField] private EnemyData enemyData;
        [SerializeField] public EnemyAnimator enemyAnimator;
        protected override float AttackTimerMax => 1 / Stats.GetStat(Stat.AttackSpeed);
        public string EnemyName => enemyData == null ? "" : enemyData.enemyName;

        #endregion

        //Set enemy data, called on every client on spawn
        private void SetEnemyData(EnemyData data)
        {
            enemyData = data;

            Team = Team.Enemy;

            var hitbox = GetComponent<CircleCollider2D>();
            var coll = GetComponent<BoxCollider2D>();

            if (hitbox != null) hitbox.radius = enemyData.hitboxRadius;
            if (coll != null) coll.size = enemyData.hitboxRadius * Vector2.one;

            enemyAnimator.Set(enemyData);
            SubscribeAnimatorEvents();
            
            InitContactDamage(enemyData.contactAttackData);
            
            Init();
            
            CmdAddEffect(new EnemyScalingEffect(), this);
            EnemyModifiers.ModifyEnemy(this);
        }

        private void InitContactDamage(AttackData contactAttackData)
        {
            AttackFactory.CreateStaticAttack(this, Vector2.zero, contactAttackData, true, null);
        }
        
        //Initialize variables and events
        private void Start()
        {
            SetEnemyData(enemyData);
            
            enemyAnimator = GetComponent<EnemyAnimator>();

            if(isServer) OnDeath += (value) => EnemyDie();
        }

        //Sets up the animator to play certain animations on certain events
        private void SubscribeAnimatorEvents()
        {
            OnLocalHurt += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Hurt);
            if (!EntityOwnership) return;
            OnDeath += (float value) => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Die);
            OnAttack += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Attack);
            //_enemyMovementBase.OnStartMove += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Move);
            //_enemyMovementBase.OnStopMove += () => enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Stop);
        }

        //Give money reward and destroy self.
        private void EnemyDie()
        {
            GameManager.Instance.CmdSpawnMoney(enemyData.baseMoneyReward, transform.position, CurrentParentIdentity);
        }
    }
}