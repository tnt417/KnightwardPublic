using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Towers;
using UnityEngine;

namespace TonyDev
{
    public class ProjectileTower : Tower
    {
        //Editor variables
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
            if(rotateToFaceTargetObject != null) rotateToFaceTargetObject.transform.right = Direction;
            var myPosition = transform.position;
            var projectile = Instantiate(projectilePrefab); //Instantiates the projectile
            projectile.transform.position = myPosition; //Set the projectile's position to our enemy's position
            var rb = projectile.GetComponent<Rigidbody2D>();
            if(setProjectileRotation) projectile.transform.right = Direction; //Set the projectile's direction
            rb.velocity = Direction * projectileTravelSpeed; //Set the projectile's velocity
        }
    }
}
