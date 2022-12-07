using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Behavior
{
    public class EnemyBehavior : GameBehavior
    {
        protected Enemy Enemy;
        protected Rigidbody2D Rb2d;

        protected Transform FirstEnemyTarget => Enemy.Targets.Count > 0 ? Enemy.Targets[0] != null ? Enemy.Targets[0].transform : null : null;

        protected Vector2 GetDirectionToFirstTarget()
        {
            if (Enemy.Targets.Count == 0) return Vector2.zero;
            
            return (Enemy.Targets[0].transform.position - transform.position).normalized;
        }
        
        protected async UniTask FollowForSeconds(Func<Transform> followTransform, Func<float> speed, float seconds)
        {
            var doneTime = Time.time + seconds;

            await FollowUntil(followTransform, speed, () => Time.time > doneTime);
        }

        protected async UniTask FollowUntil(Func<Transform> followTransform, Func<float> speed, Func<bool> predicate)
        {
            while (!predicate.Invoke())
            {
                await UniTask.WaitForFixedUpdate();
                
                var t = followTransform.Invoke();
                
                if (t == null) continue;

                Rb2d.velocity = t != null ? (followTransform.Invoke().position - transform.position).normalized * speed.Invoke() : Vector2.zero;
            }

            Rb2d.velocity = Vector2.zero;
        }
        
        protected async UniTask GotoOverSeconds(Vector2 position, float seconds)
        {
            var doneTime = Time.time + seconds;
            
            var directionVectorUnNormalized = position - (Vector2) transform.position;
            var directionVectorNormalized = directionVectorUnNormalized.normalized;
            var distanceTravelling = directionVectorUnNormalized.magnitude;
            
            //Need to divide distance into the distance-per-seconds that we have to travel.
            
            while (Time.time < doneTime)
            {
                await UniTask.WaitUntil(() => Enemy.Targets.Count > 0);
                Rb2d.velocity = directionVectorNormalized * (distanceTravelling/seconds);
                await UniTask.WaitForFixedUpdate();
            }
        }

        protected async UniTask Goto(Vector2 position, Func<float> speed, float maxTime = -1)
        {
            var endTime = maxTime < 0 ? Mathf.Infinity : Time.time + maxTime;
            
            while (!((Vector2) transform.position).Equals(position) && Time.time < endTime)
            {
                transform.position = Vector2.MoveTowards(transform.position, position, speed.Invoke() * Time.fixedDeltaTime);
                await UniTask.WaitForFixedUpdate();
            }
        }

        protected void ShootProjectile(ProjectileData data, Vector2 origin, Vector2 direction)
        {
            ObjectSpawner.SpawnProjectile(Enemy, origin, direction, data);
        }
        
        protected void ShootProjectileSpread(ProjectileData[] data, Vector2 origin, Vector2 direction)
        {
            foreach (var pd in data)
            {
                ObjectSpawner.SpawnProjectile(Enemy, origin, direction, pd);   
            }
        }

        protected void PlayAnimation(EnemyAnimationState state)
        {
            Enemy.enemyAnimator.PlayAnimationGlobal(state);
        }
        
        protected Vector2 FindStrafeDirection(float strafeDistance, float strafeRadius) //Some trigonometry to find a position a certain distance away from both the player and enemy
        {
            var posA = transform.position;
            var posB = Enemy.Targets[0].transform.position;
            var dist = Vector2.Distance(posA, posB);
            var angleA = Mathf.Acos(
                Mathf.Clamp(
                    (Mathf.Pow(strafeDistance, 2f) + Mathf.Pow(dist, 2f) - Mathf.Pow(strafeRadius, 2f)) /
                    (2f * strafeDistance * dist), -1, 1));
            var final = Tools.Rotate((posB - posA).normalized, angleA);
            return final;
        }

        protected async UniTask FlipSpriteToTarget()
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            
            while (Enemy != null)
            {
                await UniTask.WaitForFixedUpdate();
                if (!isActiveAndEnabled || gameObject == null || Enemy == null || Enemy.Targets.Count == 0) continue;
                sr.flipX = Enemy.Targets[0]?.transform.position.x > transform.position.x;
            }
        }

        protected new void Start()
        {
            base.Start();
            DestroyToken.RegisterRaiseCancelOnDestroy(this);
            FlipSpriteToTarget().AttachExternalCancellation(DestroyToken.Token);
        }
        
        protected void Awake()
        {
            Enemy = GetComponent<Enemy>();
            Rb2d = GetComponent<Rigidbody2D>();
        }
    }
}
