using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using UniRx;
using UnityEngine;

namespace TonyDev
{
    public class GolemChainBehavior : ProjectileBehavior
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float retractSpeed;
        private Vector2 _startPosition;
        private Vector2 _hitPosition;
        
        private bool _hit = false;
        
        private new void Start()
        {
            base.Start();
            _startPosition = transform.position;
            lineRenderer.SetPosition(0, _startPosition);
        }

        private void FixedUpdate()
        {
            if(!_hit) lineRenderer.SetPosition(1, transform.position);
        }

        protected override async UniTask ExecuteBehavior()
        {
            var ge = await OnHitEntity().First();
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<AttackComponent>().enabled = false;
            GetComponent<ProjectileMovement>().enabled = false;
            _hitPosition = ge.transform.position;
            _hit = true;
            HitAnimation().AttachExternalCancellation(DestroyToken.Token);
        }

        private async UniTask HitAnimation()
        {
            var lineDistance = Vector2.Distance(_startPosition, _hitPosition);
            var travelTime = lineDistance / retractSpeed;
            var animationEndTime = Time.time + travelTime;
            
            while (isActiveAndEnabled && Time.time < animationEndTime)
            {
                lineRenderer.SetPosition(1, Vector2.MoveTowards(lineRenderer.GetPosition(1), owner.transform.position,
                    retractSpeed * Time.fixedDeltaTime));
                await UniTask.WaitForFixedUpdate();
            }
            Destroy(gameObject);
        }
    }
}
