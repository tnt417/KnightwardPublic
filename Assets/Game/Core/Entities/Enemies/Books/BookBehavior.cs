using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Behavior;
using Vector2 = UnityEngine.Vector2;

namespace TonyDev.Game.Core.Entities.Enemies.Books
{
    public class BookBehavior : EnemyBehavior
    {
        public float dashSpeed;
        public float waitTimeSeconds;
        public float dashRadius;

        private int _dashes;
        
        protected override async UniTask ExecuteBehavior()
        {
            bool reached = false;
            
            while (true)
            {
                await UniTask.WaitForFixedUpdate();
                
                if (!isActiveAndEnabled || Enemy.Targets.Count == 0) continue;
            
                await PathfindFollowUntilDirectSight(() => FirstEnemyTarget,
                    () => Enemy.Stats.GetStat(Stat.MoveSpeed) * dashSpeed);
                
                var directionVector = ((Vector2) Enemy.Targets[0].transform.position - (Vector2) transform.position).normalized;
                if (!reached)
                {
                    var distance = Vector2.Distance(Enemy.Targets[0].transform.position, transform.position);
                    await Goto((Vector2)Enemy.Targets[0].transform.position - directionVector * dashRadius, () => Enemy.Stats.GetStat(Stat.MoveSpeed) * dashSpeed * 2, distance/(Enemy.Stats.GetStat(Stat.MoveSpeed)*dashSpeed*2));
                    reached = true;
                }
                await GotoOverSeconds((Vector2)Enemy.Targets[0].transform.position + directionVector*dashRadius, 1/(Enemy.Stats.GetStat(Stat.MoveSpeed)*dashSpeed));

                _dashes++;

                if (_dashes % 3 == 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1f));
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
            }
        }
    }
}
