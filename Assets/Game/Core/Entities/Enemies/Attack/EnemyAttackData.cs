using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    public class EnemyAttackData : ScriptableObject
    {
        [Tooltip("The cooldown in seconds of the attack. -1 to disable automatic attacking.")]
        public float attackCooldown;
        public Team team;
    }
}
