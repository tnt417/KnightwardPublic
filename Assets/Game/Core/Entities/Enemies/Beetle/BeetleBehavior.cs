using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class BeetleBehavior : EnemyBehavior
    {
        public float chaseTime;
        public float chaseSpeed;
        public float dashSpeed;
        public float dashRadius;

        public ProjectileData sandProjectile;

        private new void Start()
        {
            Enemy.OnDamageOther += ActivatePounce;
            base.Start();
        }

        private bool _shouldPounce = false;

        private GameEntity _pounceTarget;
        
        private void ActivatePounce(float dmg, GameEntity ge, bool crit, DamageType dt)
        {
            _shouldPounce = true;
            _pounceTarget = ge;
        }
        
        protected override async UniTask ExecuteBehavior()
        {
            while (Enemy.IsAlive)
            {
                if (FirstEnemyTarget != null)
                {
                    _shouldPounce = false;
                    
                    PlayAnimation(EnemyAnimationState.Move);
                    
                    var shootDirection = ((Vector2) Enemy.Targets[0].transform.position - (Vector2) transform.position).normalized;
                    for (var i = 0; i < 6; i++)
                    {
                        ShootProjectile(sandProjectile, Enemy.transform.position, GameTools.Rotate(shootDirection, (i-3) * (9 + Random.Range(-2f, 2f)) * Mathf.Deg2Rad));
                        await UniTask.Delay((int)(25f / Enemy.Stats.GetStat(Stat.AttackSpeed)));
                    }

                    await UniTask.Delay(1000);

                    if (_shouldPounce)
                    {
                        _shouldPounce = false;
                        PlayAnimation(EnemyAnimationState.Attack); // Burrow under the ground
                        await UniTask.Delay(900);
                        if (_pounceTarget != null)
                        {
                            Enemy.transform.position = Vector2.Lerp(Enemy.transform.position, _pounceTarget.transform.position, 0.8f);
                        }
                        PlayAnimation(EnemyAnimationState.Stop);
                        
                        var dir = ((Vector2) _pounceTarget.transform.position - (Vector2) transform.position).normalized;
                        await GotoOverSeconds((Vector2)Enemy.Targets[0].transform.position + dir*dashRadius, 1/(Enemy.Stats.GetStat(Stat.MoveSpeed)*dashSpeed));
                        await UniTask.Delay(1000);
                    }
                    else
                    {
                        PlayAnimation(EnemyAnimationState.Move);
                        await PathfindFollowUntilDirectSight(() => FirstEnemyTarget,
                            () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeed);
                        await FollowForSeconds(() => FirstEnemyTarget,
                            () => Enemy.Stats.GetStat(Stat.MoveSpeed) * chaseSpeed, 3);
                    }
                }

                await UniTask.WaitForFixedUpdate();
            }
        }
    }
}
