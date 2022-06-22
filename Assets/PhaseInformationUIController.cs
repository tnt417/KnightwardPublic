using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PhaseInformationUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text enemyDifficultyText;

    private void FixedUpdate()
    {
        enemiesRemainingText.text = "Enemies Remaining: " + (GameManager.EnemySpawners.Sum(s => s.destroyAfterSpawns - s.Spawns) +
                                     GameManager.Enemies.Count);
        enemyDifficultyText.text = "Difficulty Scale: " + GameManager.EnemyDifficultyScale;
    }
}
