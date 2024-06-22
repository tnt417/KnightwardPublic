using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class CoolEffect : AbilityEffect
    {
        public float range = 20f;

        private bool _going = false;
        private GameEntity _currentTarget;

        protected override void OnAbilityActivate()
        {
            _going = true;
        }

        public override void OnUpdateClient()
        {
            // if (_currentTarget == null)
            // {
            //     _currentTarget = GameManager.GetEntitiesInRange(Entity.transform.position, range).Max(ge =>
            //         Vector2.Distance(Entity.transform.position, ge.transform.position));
            // }
        }

        protected override void OnAbilityDeactivate()
        {
            _going = false;
        }
    }
}