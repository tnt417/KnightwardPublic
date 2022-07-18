using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities.Towers;
using UnityEngine;

namespace TonyDev
{
    public enum TowerBuffType
    {
        Strength, FireSpeed
    }
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

        private readonly Dictionary<string, float> _strengthBuffs = new();
        private readonly Dictionary<string, float> _fireSpeedBuffs = new();

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
                foreach (var buff in _strengthBuffs)
                {
                    dmg.damageMultiplier *= buff.Value;
                }

                projectile.transform.position = transform.position + (Vector3)projectileOriginOffset; //Set the projectile's position to our tower's position
                var rb = projectile.GetComponent<Rigidbody2D>();
                if (setProjectileRotation) projectile.transform.right = direction; //Set the projectile's direction
                rb.velocity = direction * projectileTravelSpeed; //Set the projectile's velocity
            }
        }

        private void UpdateMultipliers()
        {
            AttackSpeedMultiplier = 1;
            foreach (var buff in _fireSpeedBuffs)
            {
                AttackSpeedMultiplier *= buff.Value;
                Debug.Log(AttackSpeedMultiplier);
            }
        }

        public void AddBuff(TowerBuffType type, string id, float strength)
        {
            if(type == TowerBuffType.Strength) _strengthBuffs.Add(id, strength);
            else _fireSpeedBuffs.Add(id, strength);
            UpdateMultipliers();
        }

        public void RemoveBuff(string id)
        {
            _strengthBuffs.Remove(id);
            _fireSpeedBuffs.Remove(id);
            UpdateMultipliers();
        }
    }
}
