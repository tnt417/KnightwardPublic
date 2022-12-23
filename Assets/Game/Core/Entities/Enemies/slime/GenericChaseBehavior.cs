using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using UniRx;
using UnityEngine;

namespace TonyDev
{
    public class GenericChaseBehavior : EnemyBehavior
    {
        public float chaseSpeedMultiplier;

        protected override async UniTask ExecuteBehavior()
        {
            while (Enemy != null)
            {
                await UniTask.WaitForFixedUpdate();
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;

                Enemy.enemyAnimator.PlayAnimationGlobal(EnemyAnimationState.Move);

                await PathfindFollowUntilDirectSight(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier);

                await FollowUntil(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier,
                    () => !RaycastForTransform(FirstEnemyTarget));
            }
        }
    }
}