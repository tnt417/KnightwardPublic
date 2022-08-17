using System;
using Mirror;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.ScriptableObjects
{
    public enum EnemyMovementType
    {
        Chase, Strafe
    }

    public class EnemyMovementData : ScriptableObject
    {
        public float baseSpeed;
        public bool doXFlipping;
    }
}
