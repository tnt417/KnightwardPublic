using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class CelestialTowerEffect : GameEffect
    {
        private Dictionary<Tower, ParticleTrailEffect> _trailEffects = new();

        public string buffEffectPrefabKey;
        public Stat[] validStatTypes;
        public int StatBonusCount;
        public float baseBonusMagnitude;

        [HideInInspector] public StatBonus[] Buffs;

        public override void OnAddOwner()
        {
            Entity.OnTargetChangeOwner += DoBuffing;
            Entity.OnDeathOwner += UndoBuffing;
        }

        private void DoBuffing()
        {
            foreach (var t in Entity.Targets.Select(t => t.GetComponent<Tower>()).Where(t => t != null))
            {
                foreach (var sb in Buffs)
                {
                    if (!_trailEffects.ContainsKey(t))
                    {
                        _trailEffects[t] = new ParticleTrailEffect
                        {
                            OverridePrefab = ObjectFinder.GetNameOfPrefab(ObjectFinder.GetPrefab(buffEffectPrefabKey)),
                            VisibleGlobal = true
                        };
                        t.CmdAddEffect(_trailEffects[t], Entity);
                    }

                    t.Stats.AddBuff(sb, GameEntity.EntityTargetUpdatingRate);
                }
            }
        }

        public override void OnRegisterLocal()
        {
            base.OnRegisterLocal();

            Buffs = new StatBonus[StatBonusCount];

            for (var i = 0; i < StatBonusCount; i++)
            {
                Buffs[i] = new StatBonus(StatType.AdditivePercent, GameTools.SelectRandom(validStatTypes),
                    LinearScale(baseBonusMagnitude, baseBonusMagnitude * 2f, 50), EffectIdentifier);
            }
        }

        private void UndoBuffing(float value)
        {
            foreach (var t in _trailEffects.Keys)
            {
                t.CmdRemoveEffect(_trailEffects[t]);
                t.Stats.RemoveStatBonuses(EffectIdentifier);
            }

            _trailEffects.Clear();
        }

        public override string GetEffectDescription()
        {
            return "Grants stat bonuses to towers in range:\n" +
                   PlayerStats.GetStatsTextFromBonuses(Buffs, true, true, true);
        }
    }
}