using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using UniRx;
using UnityEngine;

namespace TonyDev
{
    public class FireMageBehavior : EnemyBehavior
    {
        public float pursueSpeed;
        public float strafeDistance;
        public float strafeRadius;
        public float strafeSpeed;
        public float maxShootDistance;
        public ProjectileData[] shootProjectiles;
        
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
                await UniTask.WaitForFixedUpdate();
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;
                
                PlayAnimation(EnemyAnimationState.Move);
                await PathfindFollowUntilDirectSight(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * pursueSpeed);
                
                await GotoOverSeconds((Vector2)transform.position + FindStrafeDirection(strafeDistance, strafeRadius) * strafeDistance, 1 / (Enemy.Stats.GetStat(Stat.MoveSpeed) * strafeSpeed));
                PlayAnimation(EnemyAnimationState.Stop);
                if (FirstEnemyTarget != null &&
                    Vector2.Distance(FirstEnemyTarget.transform.position, Enemy.transform.position) < maxShootDistance)
                {
                    await ShootAnimation().First();

                    var initialTarget = FirstEnemyTarget;
                    
                    for (int i = 0; i < 3; i++)
                    {
                        if (initialTarget == null) break;
                        
                        // Record the initial position
                        var targetPosStart = initialTarget.position;

                        // Wait for the FixedUpdate timing
                        await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);

                        // Record the updated position
                        var targetPosEnd = initialTarget.position;

                        // Calculate direction and tiles per second
                        var direction = (targetPosEnd - targetPosStart).normalized;
                        var tilesPerSecond = Vector2.Distance(targetPosStart, targetPosEnd) / Time.fixedDeltaTime;

                        // Shoot projectiles
                        ShootProjectileSpread(
                            shootProjectiles,
                            targetPosEnd + direction * tilesPerSecond * 0.3f,
                            GetDirectionToFirstTarget()
                        );

                        // Delay between each shot
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
