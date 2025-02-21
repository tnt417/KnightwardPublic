using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
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

                await PathfindFollowUntilWithinTile(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier);

                await FollowUntil(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier,
                    () => FirstEnemyTarget != null && Vector2.Distance(FirstEnemyTarget.position, Enemy.transform.position) < 0.5f);
            }
        }
    }
}