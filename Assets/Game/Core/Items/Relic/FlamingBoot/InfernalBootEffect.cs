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
            Player.LocalInstance.playerMovement.OnPlayerMove += AddMagnitude;
        }

        public override void OnRemoveOwner()
        {
            Player.LocalInstance.playerMovement.OnPlayerMove -= AddMagnitude;
        }

        private double _distanceMoved;

        private AttackData _attackData = new AttackData()
        {
            damageMultiplier = 0.1f,
            destroyOnApply = false,
            hitboxRadius = 0.75f,
            knockbackMultiplier = 0f,
            lifetime = 3f,
            team = Team.Player
        };

        private void AddMagnitude(Vector2 v2)
        {
            _distanceMoved += v2.magnitude;
        }
        
        public override void OnUpdateOwner()
        {
            if (_distanceMoved < 0.5f) return;
            _distanceMoved = 0f;
            AttackFactory.CreateStaticAttack(Entity, Entity.transform.position, _attackData, false, ObjectFinder.GetPrefab("firePath"));
        }
    }
}
