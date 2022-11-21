using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
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
                
                await FollowForSeconds(() => FirstEnemyTarget, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier, Mathf.Infinity);
            }
        }
    }
}