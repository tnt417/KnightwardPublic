using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class ProjectileTower : Tower
    {
        //Editor variables
        [SerializeField] private Vector2 projectileOriginOffset;
        [SerializeField] private Vector2 projectileTargetOffset;
        [SerializeField] private GameObject rotateToFaceTargetObject; //Rotates to face direction of firing
        [SerializeField] private GameObject projectilePrefab; //Prefab spawned upon fire
        [SerializeField] private bool setProjectileRotation = true;
        [SerializeField] private float projectileTravelSpeed; //Travel speed in units per second of projectiles
        //

        protected override void TowerUpdate()
        {
            
        }

        protected override void OnFire()
        {
            towerAnimator.PlayAnimation(TowerAnimationState.Fire);

            foreach (var direction in Targets.Where(t => t != null).Select(t => (t.transform.position + (Vector3) projectileTargetOffset - transform.position).normalized))
            {
                if(rotateToFaceTargetObject != null) rotateToFaceTargetObject.transform.right = direction;
                var projectile = Instantiate(projectilePrefab); //Instantiates the projectile

                var dmg = projectile.GetComponent<DamageComponent>();

                dmg.Owner = gameObject;
                dmg.damageMultiplier *= 1 + Buff.GetStatMultiplyBonus(Stat.Damage);

                projectile.transform.position = transform.position + (Vector3)projectileOriginOffset; //Set the projectile's position to our tower's position
                var rb = projectile.GetComponent<Rigidbody2D>();
                if (setProjectileRotation) projectile.transform.right = direction; //Set the projectile's direction
                rb.velocity = direction * projectileTravelSpeed; //Set the projectile's velocity
            }
        }
    }
}
