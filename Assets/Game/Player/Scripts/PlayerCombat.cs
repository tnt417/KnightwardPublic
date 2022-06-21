using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerCombat : MonoBehaviour
{
    //Editor variables
    [SerializeField] private PlayerSlashController playerSlashController;
    
    public static PlayerCombat Instance;

    private void Awake()
    {
        //Singleton code
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
        //
    }

    private void Update()
    {
        playerSlashController.enabled = Player.Instance.IsAlive; //Disabled slashing when the player is dead
    }

    //Getters for all the combat stats, used to ensure uniform calculation in all game systems
    #region CombatStatGetters
    public static float GetAttackSpeedMultipler()
    {
        return 1 + PlayerStats.GetStatBonus(Stat.AttackSpeed); //Return attack speed multiplier based on the player's stat
    }
    
    public static float GetDamageMultiplier()
    {
        float multiplier = 1;
        if (Random.Range(0, 100) < PlayerStats.GetStatBonus(Stat.CritChance)) multiplier *= 2 + PlayerStats.GetStatBonus(Stat.CritDamage); //Apply crit calculations
        multiplier = multiplier * PlayerStats.GetStatBonus(Stat.Damage); //Apply standard damage multiplier
        return 1 + multiplier;
    }
    #endregion
}
