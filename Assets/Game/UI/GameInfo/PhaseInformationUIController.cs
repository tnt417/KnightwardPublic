using System;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.UI.GameInfo
{
    public class PhaseInformationUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text enemiesRemainingText;
        [SerializeField] private TMP_Text enemyDifficultyText;

        private void Awake()
        {
            GameManager.OnEnemyAdd += UpdateEnemyCount;
            GameManager.OnEnemyRemove += UpdateEnemyCount;
        }

        private void FixedUpdate()
        {
            enemyDifficultyText.text = "Difficulty Scale: " + GameManager.EnemyDifficultyScale + "\n" + "Dungeon Floor: " + GameManager.DungeonFloor;
        }

        private void UpdateEnemyCount(GameEntity entity)
        {
            enemiesRemainingText.text = "Enemies Remaining: " + (GameManager.EnemySpawners.Sum(s => s.destroyAfterSpawns - s.Spawns) +
                                                                 GameManager.EntitiesReadonly.Where(e => e is Enemy).ToArray().Length);
        }
    }
}
