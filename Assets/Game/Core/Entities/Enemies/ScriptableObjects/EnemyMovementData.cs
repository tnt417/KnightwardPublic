using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.ScriptableObjects
{
    public enum EnemyMovementType
    {
        Chase, Strafe
    }
    
    public class EnemyMovementData : ScriptableObject
    {
        public float speedMultiplier;
        public bool doXFlipping;
    }
}
