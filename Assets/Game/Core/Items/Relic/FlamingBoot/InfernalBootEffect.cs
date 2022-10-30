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
        public float DamageMultiplier = 0.1f;
        
        private Rigidbody2D _rb2d;
        
        public override void OnAddOwner()
        {
            _rb2d = Entity.GetComponent<Rigidbody2D>();
            _attackData.team = Entity.Team;
            _attackData.damageMultiplier = DamageMultiplier;
        }

        private double _distanceMoved;

        private AttackData _attackData = new AttackData()
        {
            damageMultiplier = 0.1f,
            destroyOnApply = false,
            hitboxRadius = 0.75f,
            knockbackMultiplier = 0f,
            lifetime = 3f,
            team = Team.Player,
            ignoreInvincibility = true
        };

        private void AddMagnitude(Vector2 v2)
        {
            _distanceMoved += v2.magnitude;
        }
        
        public override void OnUpdateOwner()
        {
            AddMagnitude(_rb2d.velocity * Time.deltaTime);
            if (_distanceMoved < 0.5f) return;
            _distanceMoved = 0f;
            AttackFactory.CreateStaticAttack(Entity, Entity.transform.position, _attackData, false, ObjectFinder.GetPrefab("firePath"));
        }
    }
}
