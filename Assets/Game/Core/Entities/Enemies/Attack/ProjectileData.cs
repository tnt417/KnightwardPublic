using System;
using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    [Serializable]
    public struct ProjectileData
    {
        [Header("Prefab - can be left blank for default")]
        public GameObject prefab;
        
        [Header("General Projectile Data")]
        public Sprite projectileSprite;
        public float damageMultiplier;
        public float hitboxRadius;
        public float knockbackMultiplier;
        public Team team;
        public bool destroyOnApply;
        public GameObject spawnOnDestroy;
        
        [Header("Projectile Movement Data")]
        public float travelSpeed;
        public float lifetime;
        public float offsetDegrees;
        public float waveAmplitude;
        public float waveFrequency;
        public float waveDistance;
    }
}
