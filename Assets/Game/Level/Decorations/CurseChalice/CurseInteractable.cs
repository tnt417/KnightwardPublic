using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations;
using UnityEngine;

namespace TonyDev
{
    public class CurseInteractable : Interactable
    {
        private static Dictionary<StatBonus, int> _possibleBonuses;

        static CurseInteractable()
        {
            GameManager.OnGameManagerAwake += () => _possibleBonuses = new()
            {
                {new StatBonus(StatType.Flat, Stat.Armor, 20, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.Damage, 5, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.Dodge, 0.05f, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.Health, 20, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.AoeSize, 0.10f, "Curse"), 1},
                {new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, 0.15f, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.CooldownReduce, 0.1f, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.CritChance, 0.1f, "Curse"), 1},
                {new StatBonus(StatType.Flat, Stat.MoveSpeed, 0.5f, "Curse"), 1}
            };
        }
    
        protected override void OnInteract(InteractType type)
        {
            _possibleBonuses = GameTools.SelectRandomNoRepeats(_possibleBonuses, out var bonus);
            _possibleBonuses = GameTools.SelectRandomNoRepeats(_possibleBonuses, out var curse);
            ObjectSpawner.SpawnTextPopup(transform.position, "<color=green>" + PlayerStats.StatLabelKey[bonus.stat] +" up</color>, <color=red>" + PlayerStats.StatLabelKey[curse.stat] + " down.</color>", Color.white, 0.2f);
            Player.LocalInstance.Stats.AddStatBonus(bonus.statType, bonus.stat, bonus.strength*2, bonus.source);
            Player.LocalInstance.Stats.AddStatBonus(curse.statType, curse.stat, -curse.strength, curse.source);
            isInteractable = false;
            PlayInteractSound();
        }
    }
}
