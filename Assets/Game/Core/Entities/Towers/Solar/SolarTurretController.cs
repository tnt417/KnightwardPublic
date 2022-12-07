using System;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
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
            animator.speed = tower.Stats.GetStat(Stat.AttackSpeed);
        }

        public void DoHarming()
        {
            if (!tower.EntityOwnership) return;
            
            foreach (var ge in GameManager.EntitiesReadonly.Where(e =>
                e.Team == Team.Enemy && e.CurrentParentIdentity == tower.CurrentParentIdentity && Vector2.Distance(e.transform.position, tower.transform.position) < harmRadius))
            {
                ge.CmdDamageEntity(tower.Stats.GetStat(Stat.Damage), false, null, false);
            }
        }
    }
}