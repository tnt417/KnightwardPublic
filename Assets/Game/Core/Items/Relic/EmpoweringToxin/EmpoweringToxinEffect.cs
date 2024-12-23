using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.EmpoweringToxin
{
    public class EmpoweringToxinEffect : GameEffect
    {
        private float MoveSpeedStrength => LinearScale(2, 5, 50);
        private float CritChanceStrength => DiminishingScale(0.3f, 0.5f, 50);
        private float AttackSpeedStrength => DiminishingScale(0.4f, 0.75f, 50);


        private float _duration = 8f;
        private float _endTime;

        private List<NetworkIdentity> _activatedRooms = new();
        
        public override void OnAddOwner()
        {
            Entity.OnParentIdentityChange += OnNewRoomEnter;
            WaveManager.Instance.OnWaveBegin += OnWaveBegin;
        }
        
        private void OnAbilityActivate()
        {
            PlayerStats.Stats.RemoveBuffsOfSource("EmpoweringToxin");
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.Flat, Stat.MoveSpeed, MoveSpeedStrength, "EmpoweringToxin"),
                _duration);
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.Flat, Stat.CritChance, CritChanceStrength, "EmpoweringToxin"), _duration);
            PlayerStats.Stats.AddBuff(
                new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, AttackSpeedStrength, "EmpoweringToxin"),
                _duration);
        }

        private void OnWaveBegin(int wave)
        {
            if (Entity.CurrentParentIdentity == null)
            {
                OnAbilityActivate();
            }
        }

        private void OnNewRoomEnter(NetworkIdentity newRoom)
        {
            if (newRoom == null || _activatedRooms.Contains(newRoom)) return;
            
            _activatedRooms.Add(newRoom);
            OnAbilityActivate();
        }

        public override string GetEffectDescription()
        {
            return $"<color=#63ab3f>When entering a new room or when a wave spawns while you are in the arena, gain {GameTools.WrapColor($"{MoveSpeedStrength:P0}", Color.yellow)} " +
                   $"move speed, {GameTools.WrapColor($"{CritChanceStrength:P0}", Color.yellow)} crit chance, and {GameTools.WrapColor($"{AttackSpeedStrength:P0}", Color.yellow)} attack speed for {GameTools.WrapColor($"{_duration} seconds", Color.white)}.</color>";
        }
    }
}