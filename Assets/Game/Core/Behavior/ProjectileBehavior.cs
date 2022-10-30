using System;
using TonyDev.Game.Core.Entities;
using UniRx;
using UnityEngine;

namespace TonyDev.Game.Core.Behavior
{
    public class ProjectileBehavior : GameBehavior
    {
        private Subject<GameEntity> m_OnHitEntity = new();
        protected IObservable<GameEntity> OnHitEntity() => m_OnHitEntity;
        
        [HideInInspector] public GameEntity owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var ge = other.GetComponent<GameEntity>();

            if (ge == null || ge == owner) return;

            m_OnHitEntity.OnNext(ge);
        }
    }
}
