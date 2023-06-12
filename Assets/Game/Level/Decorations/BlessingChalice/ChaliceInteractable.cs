using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.BlessingChalice
{
    public class ChaliceInteractable : Interactable
    {
        private static Dictionary<StatBonus, int> _possibleBonuses;

        static ChaliceInteractable()
        {
            GameManager.OnGameManagerAwake += () => _possibleBonuses = new()
            {
                {new StatBonus(StatType.Flat, Stat.Armor, 20, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.Damage, 5, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.Dodge, 0.05f, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.Health, 20, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.AoeSize, 0.10f, "Chalice"), 1},
                {new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, 0.15f, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.CooldownReduce, 0.1f, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.CritChance, 0.1f, "Chalice"), 1},
                {new StatBonus(StatType.Flat, Stat.MoveSpeed, 0.5f, "Chalice"), 1}
            };
        }

        protected override void OnInteract(InteractType type)
        {
            _possibleBonuses = GameTools.SelectRandomNoRepeats(_possibleBonuses, out var bonus);
            ObjectSpawner.SpawnTextPopup(transform.position, PlayerStats.StatLabelKey[bonus.stat] + " up!", Color.green,
                0.4f);
            Player.LocalInstance.Stats.AddStatBonus(bonus.statType, bonus.stat, bonus.strength, bonus.source);
            isInteractable = false;
            PlayInteractSound();
        }
    }
}