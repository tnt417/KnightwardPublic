using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Solar
{
    public class SolarTurretController : MonoBehaviour
    {
        [SerializeField] private Tower tower;
        [SerializeField] private Animator animator;
        [SerializeField] private float harmRadius;

        public void Update()
        {
            var spd = tower.Stats.GetStat(Stat.AttackSpeed);
            if (spd == 0)
            {
                animator.Play("SolarIdle");
            }
            
            animator.speed = spd;
        }

        public void DoHarming()
        {
            if (!tower.EntityOwnership) return;
            
            tower.SubtractDurability(1);
            
            foreach (var ge in GameManager.EntitiesReadonly.Where(e =>
                e.Team == Team.Enemy && e.CurrentParentIdentity == tower.CurrentParentIdentity && Vector2.Distance(e.transform.position, tower.transform.position) < harmRadius))
            {
                ge.CmdDamageEntity(tower.Stats.GetStat(Stat.Damage), false, null, false, DamageType.AoE);
            }
        }
    }
}