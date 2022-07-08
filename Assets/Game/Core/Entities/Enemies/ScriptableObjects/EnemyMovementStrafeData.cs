using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev
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
