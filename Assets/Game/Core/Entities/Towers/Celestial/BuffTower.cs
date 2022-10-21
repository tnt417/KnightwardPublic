using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Celestial
{
    public class BuffTower : Tower
    {
        public List<StatBonus> buffs;
        public GameObject buffEffectPrefab;

        private Dictionary<Tower, ParticleTrailEffect> _trailEffects = new();

        private void Start()
        {
            if (!EntityOwnership) return;

            OnTargetChangeOwner += DoBuffing;
        }

        private void DoBuffing()
        {
            foreach (var t in Targets.Select(t => t.GetComponent<Tower>()).Where(t => t != null))
            {
                foreach (var sb in buffs)
                {
                    if (!_trailEffects.ContainsKey(t))
                    {
                        _trailEffects[t] = new ParticleTrailEffect
                        {
                            OverridePrefab = ObjectFinder.GetNameOfPrefab(buffEffectPrefab)
                        };
                        t.CmdAddEffect(_trailEffects[t], this);
                    }

                    t.Stats.AddBuff(sb, EntityTargetUpdatingRate);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var t in _trailEffects.Keys)
            {
                t.CmdRemoveEffect(_trailEffects[t]);
                t.Stats.RemoveStatBonuses(GetInstanceID().ToString());
            }
            _trailEffects.Clear();
        }
    }
}