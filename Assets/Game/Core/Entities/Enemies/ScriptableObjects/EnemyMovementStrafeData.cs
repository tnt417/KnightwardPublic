using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Enemy/Movement/Strafe")]
    public class EnemyMovementStrafeData : EnemyMovementData
    {
        public float strafeDistance;
        public float strafeSpeed;
        public float strafeCooldown;
        public float strafeRadius;
    }
}
