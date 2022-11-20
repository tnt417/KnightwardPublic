using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using Tools = TonyDev.Game.Global.Tools;

namespace TonyDev.Game.Core.Attacks
{
    [Serializable]
    public class AttackData
    {
        public float damageMultiplier;
        public float knockbackMultiplier = 1;
        public float hitboxRadius;
        public Team team;
        public bool destroyOnApply;
        [Tooltip("-1 for infinite")] public float lifetime;
        public string spawnOnDestroyKey;
        public bool ignoreInvincibility = false;
    }

    [Serializable]
    public class ProjectileData
    {
        [Tooltip("The key of the prefab in ObjectFinder to use for the projectile. Can be left empty.")]
        [Header("Prefab")]
        public string prefabKey;

        [Header("General Projectile Data")][JsonConverter(typeof(SpriteConverter))] public Sprite projectileSprite;
        public AttackData attackData;

        [Header("Projectile Movement Data")] public bool disableMovement;
        public float travelSpeed;
        public float offsetDegrees;
        public float waveAmplitude;
        public float waveLength;
        public float waveDistance;
        public bool childOfOwner;

        [Header("Effects")] [SerializeReference] [SerializeField]
        public List<GameEffect> effects;

        [NonSerialized] public Action<float, GameEntity, bool> OnHitOther;

        //The only place that projectiles should be spawned from. Creates a projectile using this instance of the class.
        public GameObject SpawnSelf(Vector2 position, Vector2 direction, GameEntity owner, float sizeMultiplier,
            string identifier)
        {
            var rotatedDirection =
                Tools.Rotate(direction, offsetDegrees * Mathf.Deg2Rad); //Rotates direction vector by our offset

            var alternatePrefab = ObjectFinder.GetPrefab(prefabKey);

            var projectileObject = alternatePrefab == null
                ? new GameObject("Projectile")
                : Object.Instantiate(
                    alternatePrefab); //Generate prefab if not null, otherwise create new empty GameObject.

            //Add all necessary components for projectile functionality...
            var rb = projectileObject.AddComponent<Rigidbody2D>();
            var col = projectileObject.GetComponent<CircleCollider2D>();
            if (col == null)
                col = projectileObject.AddComponent<CircleCollider2D>(); //Add collider if one doesn't already exist
            var attack = projectileObject.GetComponent<AttackComponent>();
            if (attack == null)
                attack = projectileObject.AddComponent<AttackComponent>(); //Don't override existing attack components.

            var sprite = projectileObject.GetComponentInChildren<SpriteRenderer>();
            if (sprite == null)
                sprite = projectileObject
                    .AddComponent<SpriteRenderer>(); //Add SpriteRenderer if one doesn't already exist
            var destroy = projectileObject.AddComponent<DestroyAfterSeconds>();

            var projectileBehaviors = projectileObject.GetComponents<ProjectileBehavior>();
            foreach (var pb in projectileBehaviors)
            {
                pb.owner = owner;
            }
            
            if (!disableMovement)
            {
                var move = projectileObject.AddComponent<ProjectileMovement>();

                move.Set(this); //Send our projectile data to the movement class so things like sin wave movement works.
            }

            //Populate component variables...
            destroy.seconds = attackData.lifetime;
            destroy.spawnPrefabOnDestroy = ObjectFinder.GetPrefab(attackData.spawnOnDestroyKey);
            destroy.SetOwner(owner);

            //Set the AttackComponent's data and owner.
            attack.SetData(attackData, owner);
            attack.identifier = identifier;
            attack.OnDamageDealt += OnHitOther;

            //Populate the AttackComponent's inflict effects
            foreach (var e in effects)
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

            if (childOfOwner) projectileObject.transform.parent = owner.transform;

            //Set Rigidbody configuration...
            rb.gravityScale = 0;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.isKinematic = true;
            rb.velocity = projectileObject.transform.up * travelSpeed; //Set the projectile's velocity

            GameManager.Instance.projectiles.Add(projectileObject);
            return projectileObject; //Return the GameObject we have created
        }
    }
}