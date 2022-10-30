using JetBrains.Annotations;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Attacks
{
    public static class AttackFactory
    {
        
        //TODO can probably combine these two at some point
        //Responsible for creation of all projectile attacks.
        public static GameObject CreateProjectileAttack(GameEntity owner, Vector2 pos, Vector2 direction, ProjectileData projectileData, string identifier)
        {
            var go = projectileData.SpawnSelf(pos, direction, owner, owner.Stats.GetStat(Stat.AoeSize), identifier);
            var attacks = go.GetComponentsInChildren<AttackComponent>();

            foreach (var att in attacks)
            {
                att.OnDamageDealt += (f, ge, b) => owner.OnDamageOther?.Invoke(f, ge, b);
            }

            return go;
        }

        //Responsible for creation of all non-projectile attacks.
        public static GameObject CreateStaticAttack(GameEntity owner, Vector2 pos, AttackData attackData, bool childOfOwner, [CanBeNull] GameObject prefab)
        {
            var attackObject = prefab == null ? new GameObject("Attack Object") : Object.Instantiate(prefab);

            attackObject.transform.parent = childOfOwner ? owner.transform : null;
            attackObject.layer = LayerMask.NameToLayer("Attacks");
            
            attackObject.transform.localPosition = pos;
            
            var col = attackObject.AddComponent<CircleCollider2D>();
            col.radius = attackData.hitboxRadius;
            col.isTrigger = true;

            var destroy = attackObject.AddComponent<DestroyAfterSeconds>();
            destroy.seconds = attackData.lifetime == 0 ? -1 : attackData.lifetime;

            var attack = attackObject.AddComponent<AttackComponent>();
            attack.SetData(attackData, owner);
            attack.OnDamageDealt += (f, ge, b) => owner.OnDamageOther?.Invoke(f, ge, b);

            return attackObject;
        }
    }
}