using System;
using System.Collections;
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
        public float maxSpawns;

        private void Start()
        {
            var token = new CancellationTokenSource();
            token.RegisterRaiseCancelOnDestroy(this);

            Animate().AttachExternalCancellation(token.Token);
        }

        private List<GameObject> _spawnedEnemies = new();
        
        private async UniTask Animate()
        {
            while (tower != null)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(baseDelay / tower.Stats.GetStat(Stat.AttackSpeed)));
                await UniTask.WaitUntil(() => _spawnedEnemies.Count < maxSpawns);
                animator.Play("GhostSpawn");
            }
        }

        [ServerCallback]
        public void DoSpawning()
        {
            if (!tower.EntityOwnership) return;

            var enemy = ObjectSpawner.SpawnEnemy(ObjectFinder.GetPrefab(enemyName), (Vector2)tower.transform.position + new Vector2(0, 0.5f),
                tower.CurrentParentIdentity);

            enemy.Stats.ReadOnly = false;
            enemy.Stats.AddStatBonus(StatType.Override, Stat.Damage, tower.Stats.GetStat(Stat.Damage), "GhostTower");
            
            _spawnedEnemies.Add(enemy.gameObject);

            enemy.OnDeathOwner += (_) => _spawnedEnemies.Remove(enemy.gameObject);
        }
        
        [ServerCallback]
        private void OnDestroy()
        {
            foreach (var ge in _spawnedEnemies.ToList())
            {
                ge.GetComponent<GameEntity>().Die();
            }
        }
    }
}
