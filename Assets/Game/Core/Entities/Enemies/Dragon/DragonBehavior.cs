using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using UniRx;
using UnityEngine;

namespace TonyDev
{
    public class DragonBehavior : EnemyBehavior
    {
        public float dashSpeed;
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
            while (isActiveAndEnabled && Enemy.Targets[0] != null)
            {
                await GotoOverSeconds((Vector2) transform.position + ((Vector2) Enemy.Targets[0].transform.position - (Vector2) transform.position).normalized * strafeRadius * 2, 1 / (Enemy.Stats.GetStat(Stat.MoveSpeed) * dashSpeed));
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                await GotoOverSeconds((Vector2)transform.position + FindStrafeDirection(strafeDistance, strafeRadius) * strafeDistance, 1 / (Enemy.Stats.GetStat(Stat.MoveSpeed) * strafeSpeed));
                PlayAnimation(EnemyAnimationState.Stop);
                await ShootAnimation().First();
                ShootProjectileSpread(shootProjectiles, transform.position, GetDirectionToFirstTarget());
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
