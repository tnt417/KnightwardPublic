using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class AttackEffect : GameEffect
    {
        public ProjectileData ProjectileData;

        private string _identifier;
        
        public override void OnAddOwner()
        {
            _identifier = GameManager.Instance.SpawnProjectile(Entity, Entity.transform.position, Vector2.one, ProjectileData).GetComponent<AttackComponent>().identifier;
        }

        public override void OnRemoveOwner()
        {
            GameManager.Instance.CmdDestroyProjectile(_identifier);
        }
    }
}