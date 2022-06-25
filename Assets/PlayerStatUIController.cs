using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text offensiveStatsText;
    [SerializeField] private TMP_Text defensiveStatsText;
    
    private void Update()
    {
        offensiveStatsText.text = PlayerStats.GetStatsText(new Stat[]
        {
            Stat.Damage, Stat.Knockback, Stat.Stun, Stat.AoeSize, Stat.AttackSpeed, Stat.CritChance, Stat.CritDamage
        });
        defensiveStatsText.text = PlayerStats.GetStatsText(new Stat[]
        {
            Stat.Health, Stat.HpRegen, Stat.Armor, Stat.Tenacity, Stat.MoveSpeed, Stat.DamageReduction, Stat.Dodge
        });
    }
}
