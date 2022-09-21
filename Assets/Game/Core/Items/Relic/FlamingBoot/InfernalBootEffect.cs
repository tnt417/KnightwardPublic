using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.FlamingBoot
{
    public class InfernalBootEffect : GameEffect
    {
        public override void OnAddOwner()
        {
            Player.LocalInstance.playerMovement.OnPlayerMove += (vector2) => { _distanceMoved += vector2.magnitude; };
        }

        public override void OnRemoveOwner()
        {
            
        }

        private double _distanceMoved;

        private AttackData _attackData = new AttackData()
        {
            damageMultiplier = 0.1f,
            destroyOnApply = false,
            hitboxRadius = 0.5f,
            knockbackMultiplier = 0f,
            lifetime = 3f,
            team = Team.Player
        };
        
        public override void OnUpdateOwner()
        {
            if (_distanceMoved < 0.5f) return;
            _distanceMoved = 0f;
            AttackFactory.CreateStaticAttack(Entity, Entity.transform.position, _attackData, false, ObjectFinder.GetPrefab("firePath"));
        }
    }
}
