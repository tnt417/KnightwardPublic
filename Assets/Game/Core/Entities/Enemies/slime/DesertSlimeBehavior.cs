using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.slime
{
    public class DesertSlimeBehavior : EnemyBehavior
    {
        public float chaseSpeedMultiplier;
        public float stompPauseTimeSeconds;
        public ParticleSystem stompParticles;

        private bool _stomping;
        
        protected override async UniTask ExecuteBehavior()
        {
            while (Enemy != null)
            {
                await UniTask.WaitForFixedUpdate();
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;

                await PathfindFollowUntilDirectSight(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier);
                
                await FollowUntil(() => FirstEnemyTarget, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeedMultiplier,
                    () => gameObject == null || _stomping);
            }
        }

        [ExecuteInEditMode]
        public void OnStomp()
        {
            StompTask().AttachExternalCancellation(DestroyToken.Token);
        }

        private async UniTask StompTask()
        {
            stompParticles.Play();
            _stomping = true;
            await UniTask.Delay(TimeSpan.FromSeconds(stompPauseTimeSeconds));
            _stomping = false;
        }
    }
}
