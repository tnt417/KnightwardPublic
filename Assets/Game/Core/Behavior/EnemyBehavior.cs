using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace TonyDev.Game.Core.Behavior
{
    public class EnemyBehavior : GameBehavior
    {
        protected Enemy Enemy;
        protected Rigidbody2D Rb2d;

        [SerializeField] private SpriteRenderer flipRenderer;
        [SerializeField] private bool flipFlip;
        
        protected Transform FirstEnemyTarget => Enemy.Targets.Count > 0
            ? Enemy.Targets[0] != null ? Enemy.Targets[0].transform : null
            : null;

        protected Vector2 GetDirectionToFirstTarget()
        {
            if (Enemy.Targets.Count == 0) return Vector2.zero;

            return (Enemy.Targets[0].transform.position - transform.position).normalized;
        }

        protected async UniTask PathfindFollowUntilDirectSight(Func<Transform> followTransform, Func<float> speed)
        {
            await PathfindFollow(followTransform, speed, () => !RaycastForTransform(followTransform.Invoke()));
        }

        protected bool RaycastForTransform(Transform target)
        {
            if (target == null) return false;
            var hit = Physics2D.Raycast(Enemy.transform.position, (FirstEnemyTarget.transform.position - transform.position),Mathf.Infinity, LayerMask.GetMask("Player", "Level", "Crystal"));
            return hit.transform != null && target != null && hit.transform.root == target;
        }
        
        protected async UniTask PathfindFollow(Func<Transform> followTransform, Func<float> speed, Func<bool> predicate)
        {
            while (Enemy != null)
            {
                await UniTask.WaitForFixedUpdate();

                if (followTransform.Invoke() == null)
                {
                    continue; // Wait for a target
                }
                
                if (!predicate.Invoke()) return; // Check our predicate

                // Arena pathfinding or room pathfinding?
                
                Pathfinding pathfinding;
                
                if (Enemy.CurrentParentIdentity == null)
                {
                    pathfinding = Pathfinding.ArenaPathfinding;
                }
                else
                {
                    var room = RoomManager.Instance.GetRoomFromID(Enemy.CurrentParentIdentity.netId);

                    if (room == null) continue;

                    pathfinding = room.RoomPathfinding;
                }

                var t = followTransform.Invoke();
                
                Vector2 lastTargetPos = t.position;
                
                var path = pathfinding.GetPath(Enemy.transform.position,lastTargetPos);

                var pathLength = path.Count;
                
                for (var i = 1; i < pathLength; i++)
                {
                    var point = path[i];
                    
                    if (t == null) break;
                    
                    var newTargetPos = (Vector2)t.position;

                    if (lastTargetPos != newTargetPos)
                    {
                        break;
                    }
                    
                    await Goto(point, speed, 1f / speed.Invoke());
                }
            }
        }

        protected async UniTask PathfindForSeconds(Func<Transform> followTransform, Func<float> speed, float seconds)
        {
            var doneTime = Time.time + seconds;

            await PathfindFollow(followTransform, speed, () => Time.time < doneTime);
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

                Rb2d.velocity = t != null
                    ? (followTransform.Invoke().position - transform.position).normalized * speed.Invoke()
                    : Vector2.zero;
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
                Rb2d.velocity = directionVectorNormalized * (distanceTravelling / seconds);
                await UniTask.WaitForFixedUpdate();
            }
        }

        protected async UniTask Goto(Vector2 position, Func<float> speed, float maxTime = -1)
        {
            var endTime = maxTime < 0 ? Mathf.Infinity : Time.time + maxTime;

            while (!((Vector2) transform.position).Equals(position) && Time.time < endTime)
            {
                transform.position =
                    Vector2.MoveTowards(transform.position, position, speed.Invoke() * Time.fixedDeltaTime);
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

        protected Vector2
            FindStrafeDirection(float strafeDistance,
                float strafeRadius) //Some trigonometry to find a position a certain distance away from both the player and enemy
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
            while (Enemy != null)
            {
                await UniTask.WaitForFixedUpdate();
                if (!isActiveAndEnabled || gameObject == null || Enemy == null || Enemy.Targets.Count == 0) continue;
                
                var flipX = Enemy.Targets[0]?.transform.position.x > transform.position.x;
                if (flipFlip) flipX = !flipX;
                flipRenderer.flipX = flipX;
            }
        }

        protected new void Start()
        {
            if (flipRenderer == null) flipRenderer = GetComponentInChildren<SpriteRenderer>();
            
            base.Start();
            DestroyToken.RegisterRaiseCancelOnDestroy(this);
            FlipSpriteToTarget().AttachExternalCancellation(DestroyToken.Token);
        }

        protected void Awake()
        {
            Enemy = transform.GetComponent<Enemy>();
            Rb2d = transform.GetComponent<Rigidbody2D>();
        }
    }
}