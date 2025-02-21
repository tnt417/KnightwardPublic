using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.FlamingBoot
{
    public class InfernalBootEffect : GameEffect
    {
        public float DamageMultiplier = 0.1f;
        
        private Rigidbody2D _rb2d;
        //private GameObject _particleObject;
        
        public override void OnAddClient()
        {
            _rb2d = Entity.GetComponent<Rigidbody2D>();
            _attackData.team = Entity.Team;
            _attackData.damageMultiplier = DamageMultiplier;
            //_particleObject = Object.Instantiate(ObjectFinder.GetPrefab("fireBootTrail"), Entity.transform);
        }

        private double _distanceMoved;
        private Vector2 _lastPos;

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

        /*public override void OnUpdateClient()
        {
            var moveDirection = ((Vector2)Entity.transform.position - _lastPos).normalized;

            if (moveDirection == Vector2.zero) return;

            _particleObject.transform.rotation = Quaternion.LookRotation(moveDirection, new Vector3(0, 0, -1));
        }

        public override void OnRemoveClient()
        {
            Object.Destroy(_particleObject);
        }*/

        public override void OnUpdateOwner()
        {
            Vector2 newPos = Entity.transform.position;
            _distanceMoved += (_lastPos - newPos).magnitude;
            _lastPos = newPos;
            if (_distanceMoved < 0.5f) return;
            _distanceMoved = 0f;
            AttackFactory.CreateStaticAttack(Entity, (Vector2)Entity.transform.position - new Vector2(0, 0.5f), _attackData, false, ObjectFinder.GetPrefab("firePath"));
        }
    }
}
