using System.Collections.Generic;
using TonyDev.Game.Core.Combat;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Attack
{
    [CreateAssetMenu(menuName = "Enemy/Attack/Projectile")]
    public class EnemyAttackProjectileData : EnemyAttackData
    {
        public List<ProjectileData> projectileDatas;
    }
}
