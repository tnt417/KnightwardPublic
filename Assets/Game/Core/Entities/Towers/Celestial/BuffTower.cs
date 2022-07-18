using System;
using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Celestial
{
    public class BuffTower : Tower
    {
        [SerializeField] private float allyStrengthMultiplier;
        [SerializeField] private float allyAttackSpeedMultiplier;

        protected override void TowerUpdate()
        {
        }

        protected override void OnFire()
        {
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            var entity = other.GetComponent<GameEntity>();

            if (entity != null && entity is ProjectileTower projectileTower)
            {
                projectileTower.AddBuff(TowerBuffType.Strength, GetInstanceID().ToString(), allyStrengthMultiplier);
                projectileTower.AddBuff(TowerBuffType.FireSpeed, GetInstanceID().ToString(), allyAttackSpeedMultiplier);
            }
        }
        
        public void OnTriggerExit2D(Collider2D other)
        {
            var entity = other.GetComponent<GameEntity>();

            if (entity != null && entity is ProjectileTower projectileTower)
            {
                projectileTower.RemoveBuff(GetInstanceID().ToString());
            }
        }

        public void OnDestroy()
        {
            foreach (var t in FindObjectsOfType<ProjectileTower>())
            {
                t.RemoveBuff(GetInstanceID().ToString());
            }
        }
    }
}
