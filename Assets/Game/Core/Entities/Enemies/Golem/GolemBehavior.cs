using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using UniRx;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Golem
{
    public class GolemBehavior : EnemyBehavior
    {
        public float followSpeed;
        public float followTime;
        public ProjectileData throwProjectile;
        public float crystalAttackRange = 5f;

        private Subject<bool> m_ShootAnimation = new();
        private IObservable<bool> ShootAnimation() => m_ShootAnimation;

        public void Shoot() //Called in animation events
        {
            m_ShootAnimation.OnNext(true);
        }
        
        protected override async UniTask ExecuteBehavior()
        {
            while (true)
            {
                await UniTask.WaitForEndOfFrame(this);
                
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;
                
                if (Enemy.Targets[0] is Player.Player)
                {
                    PlayAnimation(EnemyAnimationState.Move);
                    await PathfindForSeconds(() => FirstEnemyTarget, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * followSpeed, followTime);
                    PlayAnimation(EnemyAnimationState.Stop);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));
                    PlayAnimation(EnemyAnimationState.Attack);
                    await ShootAnimation().First();
                    ShootProjectile(throwProjectile, transform.position, GetDirectionToFirstTarget());
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
                else
                {
                    var enemyPos = (Vector2) Enemy.Targets[0].transform.position;
                    PlayAnimation(EnemyAnimationState.Move);
                    
                    await Goto(
                        enemyPos + ((Vector2) transform.position - enemyPos).normalized *
                        crystalAttackRange, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * followSpeed, 2f);
                    PlayAnimation(EnemyAnimationState.Stop);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));
                    PlayAnimation(EnemyAnimationState.Attack);
                    await ShootAnimation().First();
                    ShootProjectile(throwProjectile, transform.position, GetDirectionToFirstTarget());
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
