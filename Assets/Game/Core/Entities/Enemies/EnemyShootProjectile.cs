using System.Collections.Generic;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyShootProjectile : MonoBehaviour
    {
        private Enemy _enemy;
        private List<Transform> Targets => _enemy.Targets;
        private ProjectileData _projectileData;
    
        // Start is called before the first frame update
        private void Start()
        {
            _enemy = GetComponentInParent<Enemy>();
            _enemy.OnAttack += ShootProjectile;
        }

        public void Set(ProjectileData projectileData, Enemy parentEnemy)
        {
            _enemy = parentEnemy;
            _projectileData = projectileData;
        }
        public void ShootProjectile() //Called through animation events
        {
            foreach (var t in Targets)
            {
                if (t == null) return;
                
                var myPosition = transform.position;
                var offset = _projectileData.offsetDegrees; //TODO: falloff
                var direction = Rotate((t.transform.position - myPosition).normalized,
                    offset * Mathf.Deg2Rad); //Calculates direction vector

                var projectileObject = _projectileData.prefab == null
                    ? new GameObject()
                    : Instantiate(_projectileData.prefab);

                var rb = projectileObject.AddComponent<Rigidbody2D>();
                var col = projectileObject.AddComponent<CircleCollider2D>();
                var dmg = projectileObject.AddComponent<DamageComponent>();
                var sprite = projectileObject.GetComponent<SpriteRenderer>();
                if (sprite == null) sprite = projectileObject.AddComponent<SpriteRenderer>();
                var spawn = projectileObject.AddComponent<DestroyAfterSeconds>();
                var move = projectileObject.AddComponent<ProjectileMovement>();

                move.Set(_projectileData);

                spawn.seconds = _projectileData.lifetime;
                spawn.spawnPrefabOnDestroy = _projectileData.spawnOnDestroy;

                dmg.damage = _projectileData.damageMultiplier;
                dmg.team = _projectileData.team;
                dmg.damageCooldown = 0.5f;
                dmg.destroyOnApply = _projectileData.destroyOnApply;
                dmg.knockbackMultiplier = _projectileData.knockbackMultiplier;

                sprite.sprite = _projectileData.projectileSprite;
                col.radius = _projectileData.hitboxRadius;
                col.isTrigger = true;

                projectileObject.transform.position =
                    myPosition; //Set the projectile's position to our enemy's position
                projectileObject.transform.up = direction; //Set the projectile's direction
                rb.gravityScale = 0;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.isKinematic = true;
                rb.velocity =
                    projectileObject.transform.up * _projectileData.travelSpeed; //Set the projectile's velocity
            }
        }
        
        //Helper function to rotate a vector by radians
        private static Vector2 Rotate(Vector2 v, float radians)
        {
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);

            var tx = v.x;
            var ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}
