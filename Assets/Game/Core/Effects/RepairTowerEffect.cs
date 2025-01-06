using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class RepairTowerEffect : GameEffect
    {
        private HashSet<Tower> _buffedTowers = new();
        private static Dictionary<Tower, ParticleTrailEffect> _trailEffects = new();
        private static Dictionary<Tower, RepairBuffAnim> _trailRbas = new();

        public float durabilityPercentPerSecond;
        public string buffEffectPrefabKey;

        [HideInInspector] public StatBonus[] Buffs;

        public override void OnAddOwner()
        {
            Entity.OnDeathOwner += UndoBuffing;
        }

        private float _timer;
        
        public override void OnUpdateServer()
        {
            _timer += Time.deltaTime;

            if (_timer >= 2)
            {
                _timer = 0;
                DoRepair();
            }
        }
        
        private void DoRepair()
        {
            foreach (var t in Entity.Targets.Select(t => t.GetComponent<Tower>()).Where(t => t != null))
            {
                    if (t.myItem.itemName == "Repair Tower" || t.MaxDurability >= Tower.InfiniteDurabilityThreshold) continue;
                
                    if (!_trailEffects.ContainsKey(t))
                    {
                        _trailEffects[t] = new ParticleTrailEffect
                        {
                            OverridePrefab = ObjectFinder.GetNameOfPrefab(ObjectFinder.GetPrefab(buffEffectPrefabKey)),
                            VisibleGlobal = true
                        };
                        t.CmdAddEffect(_trailEffects[t], Entity);
                    }
                    
                    DoBuffingTask(t).Forget();
            
                    if (!_trailRbas.ContainsKey(t))
                    {
                        _trailRbas.Add(t, t.GetComponentInChildren<RepairBuffAnim>());
                    }
                    
                    t.ApplyDamage(-Mathf.CeilToInt(durabilityPercentPerSecond * t.MaxDurability), out _);
            }
        }

        private async UniTask DoBuffingTask(Tower t)
        {
            if (t == null) return;
            
            float dist = Vector2.Distance(Entity.transform.position, t.transform.position);

            await UniTask.Delay(TimeSpan.FromSeconds(0.25 * dist));

            if (t == null || Entity == null) return;

            if (!_buffedTowers.Contains(t))
            {
                _trailRbas[t].AlterCount(1);
            }
                    
            _buffedTowers.Add(t);
        }

        private void UndoBuffing(float value)
        {
            foreach (var t in _buffedTowers)
            {
                _trailRbas[t].AlterCount(-1);
            }
            
            _buffedTowers.Clear();
        }

        public override string GetEffectDescription()
        {
            return $"Repairs nearby towers at a rate of {durabilityPercentPerSecond:P0} durability every 2 seconds.";
        }
    }
}