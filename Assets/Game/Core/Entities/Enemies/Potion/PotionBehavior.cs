using System;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Potion
{
    public class PotionBehavior : EnemyBehavior
    {
        public float followSpeed;
        public float detonateDistance;

        public ProjectileData detonateProjectile;
        
        protected override async UniTask ExecuteBehavior()
        {
            while (true)
            {
                await UniTask.WaitForEndOfFrame(this);

                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;

                PlayAnimation(EnemyAnimationState.Move);
                await FollowUntil(() => FirstEnemyTarget, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * followSpeed, () => Vector2.Distance(transform.position, Enemy.Targets[0].transform.position) <= detonateDistance);
                PlayAnimation(EnemyAnimationState.Stop);
                await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
                ShootProjectile(detonateProjectile, Enemy.transform.position, Vector2.zero);
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}