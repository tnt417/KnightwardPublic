using TMPro;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Player
{
    public class PlayerStatUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text offensiveStatsText;
        [SerializeField] private TMP_Text defensiveStatsText;
    
        private void Update()
        {
            offensiveStatsText.text = PlayerStats.GetStatsText(new Stat[]
            {
                Stat.Damage, Stat.AoeSize, Stat.AttackSpeed, Stat.CritChance
            });
            defensiveStatsText.text = PlayerStats.GetStatsText(new Stat[]
            {
                Stat.Health, Stat.HpRegen, Stat.Armor, Stat.MoveSpeed, Stat.Dodge
            });
        }
    }
}
