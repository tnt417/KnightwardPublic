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
        var multiplier = 1f;
        if (Random.Range(0f, 1f) < PlayerStats.GetStatBonus(Stat.CritChance)) multiplier *= 2f + PlayerStats.GetStatBonus(Stat.CritDamage); //Apply crit calculations
        multiplier *= 1+PlayerStats.GetStatBonus(Stat.Damage); //Apply standard damage multiplier
        return multiplier;
    }
    #endregion
}
