using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Celestial
{
    public class BuffTower : Tower
    {
        public List<StatBonus> buffs;

        private void Start()
        {
            if (!EntityOwnership) return;

            OnTargetChangeOwner += DoBuffing;
        }

        
        
        private new void Update()
        {
            base.Update();
            
        }

        private void DoBuffing()
        {
            foreach (var pt in Targets.Select(t => t.GetComponent<ProjectileTower>()).Where(pt => pt != null))
            {
                foreach (var sb in buffs)
                {
                    pt.Stats.AddBuff(sb, EntityTargetUpdatingRate);
                }
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
