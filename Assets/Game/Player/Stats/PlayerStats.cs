using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum Stat
{
    AttackSpeed, Damage, CritChance, CritDamage, AoeSize, Knockback, Stun, //Weapon stats, 0-6 //All valid for relics right now. 0-13
    MoveSpeed, Health, Armor, Tenacity, HpRegen, Dodge, DamageReduction //Armor Stats 7-13
}

[Serializable]
public struct StatBonus //Holds stat type, strength, and source.
{
    public Stat stat;
    public float strength;
    public string source;
    public StatBonus(Stat stat, float strength, string source)
    {
        this.stat = stat;
        this.strength = strength;
        this.source = source;
    }
}

public static class PlayerStats
{
    private static List<StatBonus> _activeStatBonuses = new List<StatBonus>(); //Holds all the active stat bonuses

    public static void AddStatBonus(Stat stat, float strength, string source) //Adds a stat bonus to the list.
    {
        _activeStatBonuses.Add(new StatBonus(stat, strength, source));
    }
    
    public static void AddStatBonusesFromItem(Item item) //Adds a stat bonus to the list, taking data from an item.
    {
        foreach (var sb in item.StatBonuses)
        {
            _activeStatBonuses.Add(new StatBonus(sb.stat, sb.strength, sb.source));
        }
    }

    public static void RemoveStatBonuses(string source) //Removes all stat bonuses from a specific source.
    {
        _activeStatBonuses = _activeStatBonuses.Where(sb => sb.source != source).ToList();
    }

    public static float GetStatBonus(Stat stat) //Returns a sum of all stat bonuses of a certain type.
    {
        return _activeStatBonuses.Where(sb => sb.stat == stat).Sum(sb => sb.strength);
    }

    public static Stat GetValidStatForItem(ItemType type) //Returns a random stat, based on valid stat types for different item types
    {
        return type switch
        {
            ItemType.Weapon => (Stat) Random.Range(0, 6),
            ItemType.Armor => (Stat) Random.Range(7, 13),
            ItemType.Relic => (Stat) Random.Range(0, 13),
            _ => Stat.AttackSpeed //Returns attack speed by default.
        };
    }
}
