using System;
using UnityEngine;

namespace TonyDev.Game.Core.Combat
{
    [Serializable]
    public struct ProjectileData
    {
        public Sprite projectileSprite;
        public float damage;
        public float travelSpeed;
        public float hitboxRadius;
        public float lifetime;
        public float knockbackMultiplier;
        public Team team;
        public bool destroyOnApply;
        public GameObject spawnOnDestroy;
        public Vector2 initialTravelOffset;
        public float travelOffsetFalloff;
    }
}
