using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Global;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Attacks
{
    [Serializable]
    public class AttackData
    {
        public float damageMultiplier;
        public float hitboxRadius;
        public Team team;
        public bool destroyOnApply;
        [Tooltip("-1 for infinite")] public float lifetime;
        public GameObject spawnOnDestroy;
    }

    [Serializable]
    public class ProjectileData
    {
        //[Header("Prefab")] [CanBeNull] public GameObject prefab;

        [Header("General Projectile Data")] public Sprite projectileSprite;
        public AttackData attackData;

        [Header("Projectile Movement Data")] public float travelSpeed;
        public float offsetDegrees;
        public float waveAmplitude;
        public float waveFrequency;
        public float waveDistance;

        [Header("Effects")] public List<string> effectIDs;

        //The only place that projectiles should be spawned from. Creates a projectile using this instance of the class.
        public GameObject SpawnSelf(Vector2 position, Vector2 direction, GameEntity owner, float sizeMultiplier)
        {
            var rotatedDirection =
                Tools.Rotate(direction, offsetDegrees * Mathf.Deg2Rad); //Rotates direction vector by our offset

            var projectileObject = new GameObject("Projectile");
                    //: Object.Instantiate(prefab); //Generate prefab if not null, otherwise create new empty GameObject.

            //Add all necessary components for projectile functionality...
            var rb = projectileObject.AddComponent<Rigidbody2D>();
            var col = projectileObject.GetComponent<CircleCollider2D>();
            if (col == null) col = projectileObject.AddComponent<CircleCollider2D>(); //Add collider if one doesn't already exist
            var attack = projectileObject.AddComponent<AttackComponent>();
            var sprite = projectileObject.GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) sprite = projectileObject.AddComponent<SpriteRenderer>(); //Add SpriteRenderer if one doesn't already exist
            var destroy = projectileObject.AddComponent<DestroyAfterSeconds>();
            var move = projectileObject.AddComponent<ProjectileMovement>();

            move.Set(this); //Send our projectile data to the movement class so things like sin wave movement works.

            //Populate component variables...
            destroy.seconds = attackData.lifetime;
            destroy.spawnPrefabOnDestroy = attackData.spawnOnDestroy;
            destroy.SetOwner(owner);

            //Set the AttackComponent's data and owner.
            attack.SetData(attackData, owner);

            //Populate the AttackComponent's inflict effects
            foreach (var e in effectIDs)
            {
                attack.AddInflictEffect(e);
            }

            //Set sprite and collider configuration...
            sprite.sprite = projectileSprite;
            col.radius = attackData.hitboxRadius;
            col.transform.localPosition = Vector3.zero;
            col.isTrigger = true;

            //Set transform configuration...
            projectileObject.transform.position = position; //Set the projectile's position to our enemy's position
            projectileObject.transform.up = rotatedDirection; //Set the projectile's direction
            projectileObject.transform.localScale *= sizeMultiplier;
            projectileObject.layer = LayerMask.NameToLayer("Attacks");
            
            //Set Rigidbody configuration...
            rb.gravityScale = 0;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.isKinematic = true;
            rb.velocity = projectileObject.transform.up * travelSpeed; //Set the projectile's velocity

            return projectileObject; //Return the GameObject we have created
        }
    }
}