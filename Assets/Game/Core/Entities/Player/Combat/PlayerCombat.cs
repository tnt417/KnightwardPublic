using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Items;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        public static PlayerCombat Instance;
        
        //Editor variables
        [SerializeField] private ProjectileData defaultProjectileData;
        [SerializeField] private GameObject defaultProjectilePrefab;
        //
        
        private float AttackTimerMax => PlayerStats.AttackSpeedMultiplier;
        private float _attackTimer;
        private Camera _mainCamera;
        private Vector2 Direction => (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        
        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //

            _mainCamera = Camera.main;
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= AttackTimerMax && Input.GetMouseButton(0))
            {
                Attack();
                _attackTimer = 0;
            }
        }

        private void Attack()
        {
            var projectileData = PlayerInventory.Instance.WeaponItem.projectiles;
            if (projectileData == null)
            {
                ShootProjectile(defaultProjectileData);
                return;
            }
            foreach (var proj in projectileData)
            {
                ShootProjectile(proj);
            }
        }

        private void ShootProjectile(ProjectileData data)
        {
            var myPosition = transform.position;
            var offset = data.offsetDegrees;
            var direction = Rotate(Direction, offset * Mathf.Deg2Rad); //Calculates direction vector

            var projectileObject = data.prefab == null ? Instantiate(defaultProjectilePrefab) : Instantiate(data.prefab);

            var rb = projectileObject.AddComponent<Rigidbody2D>();
            var col = projectileObject.AddComponent<CircleCollider2D>();
            var dmg = projectileObject.AddComponent<DamageComponent>();
            var sprite = projectileObject.GetComponentInChildren<SpriteRenderer>();
            if(sprite == null) sprite = projectileObject.AddComponent<SpriteRenderer>();
            var spawn = projectileObject.AddComponent<DestroyAfterSeconds>();
            var move = projectileObject.AddComponent<ProjectileMovement>();
            
            move.Set(data);
            
            spawn.seconds = data.lifetime;
            spawn.spawnPrefabOnDestroy = data.spawnOnDestroy;

            dmg.damage = data.damageMultiplier * PlayerStats.OutgoingDamageWithCrit;
            dmg.team = data.team;
            dmg.damageCooldown = 0.5f;
            dmg.destroyOnApply = data.destroyOnApply;
            dmg.knockbackMultiplier = data.knockbackMultiplier;

            sprite.sprite = data.projectileSprite;
            col.radius = data.hitboxRadius;
            col.isTrigger = true;

            projectileObject.transform.position = myPosition; //Set the projectile's position to our enemy's position
            projectileObject.transform.up = direction; //Set the projectile's direction
            projectileObject.transform.localScale *= PlayerStats.AoEMultiplier;
            rb.gravityScale = 0;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.isKinematic = true;
            rb.velocity = projectileObject.transform.up * data.travelSpeed; //Set the projectile's velocity
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
