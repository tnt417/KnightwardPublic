using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using UniRx;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.darkArcher
{
    public class StrafeAndShootBehavior : EnemyBehavior
    {
        public float strafeDistance;
        public float strafeRadius;
        public float strafeSpeed;
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
                
                await GotoOverSeconds((Vector2)transform.position + FindStrafeDirection(strafeDistance, strafeRadius) * strafeDistance, 1 / (Enemy.Stats.GetStat(Stat.MoveSpeed) * strafeSpeed));
                PlayAnimation(EnemyAnimationState.Stop);
                await ShootAnimation().First();
                ShootProjectileSpread(shootProjectiles, transform.position, GetDirectionToFirstTarget());
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
