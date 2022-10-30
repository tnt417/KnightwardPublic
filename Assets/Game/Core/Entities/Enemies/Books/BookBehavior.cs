using System;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Books
{
    public class BookBehavior : EnemyBehavior
    {
        public float dashSpeed;
        public float waitTimeSeconds;
        public float dashRadius;
        protected override async UniTask ExecuteBehavior()
        {
            bool reached = false;
            
            while (true)
            {
                await UniTask.WaitForFixedUpdate();
                
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;
            
                var directionVector = ((Vector2) Enemy.Targets[0].transform.position - (Vector2) transform.position).normalized;
                if (!reached)
                {
                    await Goto((Vector2)Enemy.Targets[0].transform.position - directionVector * dashRadius, dashSpeed * 2, 20f);
                    reached = true;
                }
                await GotoOverSeconds((Vector2)Enemy.Targets[0].transform.position + directionVector*dashRadius, 1/(Enemy.Stats.GetStat(Stat.MoveSpeed)*dashSpeed));
                await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
            }
        }
    }
}
