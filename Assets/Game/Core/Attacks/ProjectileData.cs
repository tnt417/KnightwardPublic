using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;
using Object = UnityEngine.Object;
using Newtonsoft.Json;

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
        public bool destroyOnCollideWall;
        [Tooltip("-1 for infinite")] public float lifetime;
        public string spawnOnDestroyKey;
        public bool inheritRotationOnSpawn = false;
        public bool ignoreInvincibility = false;

        public GameEffectList inflictEffects = new();
    }

    [Serializable]
    public class ProjectileData
    {
        [Tooltip("The key of the prefab in ObjectFinder to use for the projectile. Can be left empty.")]
        [Header("Prefab")]
        public string prefabKey;

        [Header("General Projectile Data")] [JsonConverter(typeof(SpriteConverter))]
        public Sprite projectileSprite;

        public AttackData attackData;

        [Header("Projectile Movement Data")] public bool disableMovement;
        public float travelSpeed;
        public float offsetDegrees;
        public float waveAmplitude;
        public float waveLength;
        public float waveDistance;
        public bool childOfOwner;
        public bool doNotRotate;

        [Header("Effects")] [SerializeReference] [SerializeField]
        public List<GameEffect> effects = new();

        [NonSerialized] public Action<float, GameEntity, bool, DamageType> OnHitOther;

        //The only place that projectiles should be spawned from. Creates a projectile using this instance of the class.
        public GameObject SpawnSelf(Vector2 position, Vector2 direction, GameEntity owner, float sizeMultiplier,
            string identifier)
        {
            var rotatedDirection =
                GameTools.Rotate(direction, offsetDegrees * Mathf.Deg2Rad); //Rotates direction vector by our offset

            var alternatePrefab = ObjectFinder.GetPrefab(prefabKey);

            var projectileObject = alternatePrefab == null
                ? new GameObject("Projectile")
                : Object.Instantiate(
                    alternatePrefab); //Generate prefab if not null, otherwise create new empty GameObject.

            if (childOfOwner) projectileObject.transform.parent = owner.transform;

            //Add all necessary components for projectile functionality...
            var rb = projectileObject.GetComponent<Rigidbody2D>();
            if (rb == null) rb = projectileObject.AddComponent<Rigidbody2D>();

            var col = projectileObject.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = projectileObject.AddComponent<CircleCollider2D>(); //Add collider if one doesn't already exist
                
                if (attackData.hitboxRadius <= 0) // hitboxRadius of 0 indicates that we don't want to allow collision
                    col.enabled = false;
            }

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
                move.direction = rotatedDirection;
            }

            //Populate component variables...
            destroy.seconds = attackData.lifetime;
            destroy.spawnPrefabOnDestroy = ObjectFinder.GetPrefab(attackData.spawnOnDestroyKey);
            destroy.inheritRotation = attackData.inheritRotationOnSpawn;
            destroy.SetOwner(owner);

            //Set the AttackComponent's data and owner.
            attack.SetData(attackData, owner);
            attack.identifier = identifier;
            attack.OnDamageDealt += OnHitOther;
            attack.destroyOnHitWall = attackData.destroyOnCollideWall;

            //Populate the AttackComponent's inflict effects
            if (attackData.inflictEffects is {gameEffects: { }})
                foreach (var e in attackData.inflictEffects.gameEffects)
                {
                    attack.AddInflictEffect(e);
                }

            //Set sprite and collider configuration...
            if(projectileSprite != null) sprite.sprite = projectileSprite;
            col.radius = attackData.hitboxRadius;
            col.transform.localPosition = Vector3.zero;
            col.isTrigger = true;

            //Set transform configuration...
            projectileObject.transform.position = position; //Set the projectile's position to our enemy's position
            if (!doNotRotate) projectileObject.transform.up = rotatedDirection; //Set the projectile's direction
            projectileObject.transform.localScale *= sizeMultiplier;
            projectileObject.layer = LayerMask.NameToLayer("Attacks");

            if (attackData.destroyOnCollideWall)
            {
                var go = Object.Instantiate(ObjectFinder.GetPrefab("WallDestroy"), projectileObject.transform);
                go.GetComponent<CircleCollider2D>().radius = attackData.hitboxRadius;
            }

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