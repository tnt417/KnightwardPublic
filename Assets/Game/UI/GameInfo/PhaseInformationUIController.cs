using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.UI
{
    public class PhaseInformationUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text enemiesRemainingText;
        [SerializeField] private TMP_Text enemyDifficultyText;

        private void FixedUpdate()
        {
            enemiesRemainingText.text = "Enemies Remaining: " + (GameManager.EnemySpawners.Sum(s => s.destroyAfterSpawns - s.Spawns) +
                                                                 GameManager.Entities.Where(e => e is Enemy).ToArray().Length);
            enemyDifficultyText.text = "Difficulty Scale: " + GameManager.EnemyDifficultyScale + "\n" + "Dungeon Floor: " + GameManager.DungeonFloor;
        }
    }
}
