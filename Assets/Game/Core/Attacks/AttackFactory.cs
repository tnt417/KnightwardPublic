using JetBrains.Annotations;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Attacks
{
    public static class AttackFactory
    {
        //Responsible for creation of all projectile attacks.
        public static void CreateProjectileAttack(GameEntity owner, Vector2 direction, ProjectileData projectileData)
        {
            projectileData.SpawnSelf(owner.transform.position, direction, owner, owner.Stats.GetStat(Stat.AoeSize));
        }

        //Responsible for creation of all non-projectile attacks.
        public static void CreateStaticAttack(GameEntity owner, AttackData attackData, bool child, [CanBeNull] GameObject prefab)
        {
            var attackObject = prefab == null ? new GameObject("Attack Object") : Object.Instantiate(prefab);

            attackObject.transform.parent = child ? owner.transform : null;
            attackObject.layer = LayerMask.NameToLayer("Attacks");
            
            if(child) attackObject.transform.localPosition = Vector3.zero;

            var col = attackObject.AddComponent<CircleCollider2D>();
            col.radius = attackData.hitboxRadius;
            col.isTrigger = true;

            var destroy = attackObject.AddComponent<DestroyAfterSeconds>();
            destroy.seconds = attackData.lifetime == 0 ? -1 : attackData.lifetime;

            var attack = attackObject.AddComponent<AttackComponent>();
            attack.SetData(attackData, owner);
        }
    }
}