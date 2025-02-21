using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class GhostTowerController : MonoBehaviour
    {
        public Tower tower;
        public Animator animator;
        public string enemyName;

        public float baseDelay;
        public int maxSpawns;

        private void Start()
        {
            var token = new CancellationTokenSource();
            token.RegisterRaiseCancelOnDestroy(this);

            Animate().AttachExternalCancellation(token.Token);
        }

        private List<GameEntity> _spawnedEnemies = new();
        
        private async UniTask Animate()
        {
            while (tower != null)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(baseDelay / tower.Stats.GetStat(Stat.AttackSpeed)));

                if (tower.durability <= 0) continue;
                
                _spawnedEnemies = _spawnedEnemies.Where(ge => ge != null).ToList();
                
                if (_spawnedEnemies.Count == maxSpawns) continue;
                
                if (_spawnedEnemies.Count > maxSpawns)
                {
                    var removing = _spawnedEnemies[maxSpawns-1];
                    if (removing != null)
                    {
                        removing.Die();
                    }
                    else
                    {
                        _spawnedEnemies.Remove(removing);
                    }
                }
                
                animator.Play("GhostSpawn");
            }
        }

        [ServerCallback]
        public void DoSpawning()
        {
            if (!tower.EntityOwnership) return;

            _spawnedEnemies = _spawnedEnemies.Where(ge => ge != null).ToList();
            
            if (_spawnedEnemies.Count == maxSpawns) return;
            
            if (tower.durability > 0)
            {
                tower.SubtractDurability(1);
            }
            else
            {
                return;
            }

            var enemy = ObjectSpawner.SpawnEnemy(ObjectFinder.GetPrefab(enemyName), (Vector2)tower.transform.position + new Vector2(0, 0.5f),
                tower.CurrentParentIdentity);

            enemy.Stats.ReadOnly = false;
            enemy.Stats.AddStatBonus(StatType.Override, Stat.Damage, tower.Stats.GetStat(Stat.Damage), "GhostTower");
            enemy.Stats.AddStatBonus(StatType.Flat, Stat.MoveSpeed, tower.Stats.GetStat(Stat.MoveSpeed), "GhostTower");

            _spawnedEnemies.Add(enemy);

            enemy.OnDeathOwner += (_) => _spawnedEnemies.Remove(enemy);
        }
        
        [ServerCallback]
        private void OnDestroy()
        {
            foreach (var ge in _spawnedEnemies.ToList())
            {
                ge.Die();
            }
        }
    }
}
