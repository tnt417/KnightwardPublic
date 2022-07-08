using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyShootProjectile : MonoBehaviour
    {
        private Enemy enemy;
        private Transform Target => enemy.Target;
        private ProjectileData _projectileData;
    
        // Start is called before the first frame update
        private void Start()
        {
            enemy = GetComponentInParent<Enemy>();
            enemy.OnAttack += ShootProjectile;
        }

        public void Set(ProjectileData projectileData, Enemy parentEnemy)
        {
            enemy = parentEnemy;
            _projectileData = projectileData;
        }
        public void ShootProjectile() //Called through animation events
        {
            var myPosition = transform.position;
            var travelOffset = _projectileData.initialTravelOffset; //TODO: falloff
            var direction = (Vector2)(Target.transform.position - myPosition).normalized + travelOffset; //Calculates direction vector

            var projectileObject = new GameObject();
            
            var rb = projectileObject.AddComponent<Rigidbody2D>();
            var col = projectileObject.AddComponent<CircleCollider2D>();
            var dmg = projectileObject.AddComponent<DamageComponent>();
            var sprite = projectileObject.AddComponent<SpriteRenderer>();
            var spawn = projectileObject.AddComponent<DestroyAfterSeconds>();
            
            spawn.seconds = _projectileData.lifetime;
            spawn.spawnPrefabOnDestroy = _projectileData.spawnOnDestroy;

            dmg.damage = _projectileData.damage;
            dmg.team = _projectileData.team;
            dmg.damageCooldown = 0.5f;
            dmg.destroyOnApply = _projectileData.destroyOnApply;
            dmg.knockbackMultiplier = _projectileData.knockbackMultiplier;

            sprite.sprite = _projectileData.projectileSprite;
            col.radius = _projectileData.hitboxRadius;
            col.isTrigger = true;

            projectileObject.transform.position = myPosition; //Set the projectile's position to our enemy's position
            projectileObject.transform.up = direction; //Set the projectile's direction
            rb.gravityScale = 0;
            rb.isKinematic = true;
            rb.velocity = direction * _projectileData.travelSpeed; //Set the projectile's velocity
        }
    }
}
