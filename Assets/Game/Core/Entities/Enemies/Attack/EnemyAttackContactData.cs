using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    [CreateAssetMenu(menuName = "Enemy/Attack/Contact")]
    public class EnemyAttackContactData : EnemyAttackData
    {
        public float hitboxRadius;
        public float DamageCooldown => 0.5f;
        public float damage;
        public bool destroyOnApply;
        public float knockbackMultiplier;
    }
}
