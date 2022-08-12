using System;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Celestial
{
    public class BuffTower : Tower
    {
        [SerializeField] private float allyStrengthBonusPercent;
        [SerializeField] private float allyAttackSpeedBonusPercent;

        private void Awake()
        {
            OnTargetChange += DoBuffing;
        }

        private void DoBuffing()
        {
            foreach (var pt in Targets.Select(t => t.GetComponent<ProjectileTower>()).Where(pt => pt != null))
            {
                var source = GetInstanceID().ToString();
                pt.Stats.RemoveStatBonuses(source);
                pt.Stats.AddStatBonus(StatType.Multiplicative, Stat.Damage, allyStrengthBonusPercent, source);
                pt.Stats.AddStatBonus(StatType.Multiplicative, Stat.AttackSpeed, allyAttackSpeedBonusPercent, source);
            }
        }

        public void OnDestroy()
        {
            foreach (var t in FindObjectsOfType<ProjectileTower>())
            {
                t.Stats.RemoveStatBonuses(GetInstanceID().ToString());
            }
        }
    }
}
