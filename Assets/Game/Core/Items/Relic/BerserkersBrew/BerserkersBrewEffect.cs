using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.BerserkersBrew
{
    public class BerserkersBrewEffect : GameEffect
    {
        private float RegenMultiplier => LinearScale(3, 6, 50);
        private float DamageMultiplier => LinearScale(0.8f, 1.5f, 50);
        protected void OnAbilityActivate()
        {
            PlayerStats.Stats.RemoveBuffsOfSource("BerserkersBrew");
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.HpRegen, RegenMultiplier, "BerserkersBrew"), _duration);
            PlayerStats.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, DamageMultiplier, "BerserkersBrew"), _duration);
        }

        private float _duration = 8f;
        private float _endTime;

        private List<NetworkIdentity> _activatedRooms = new();
        
        public override void OnAddOwner()
        {
            Entity.OnParentIdentityChange += OnNewRoomEnter;
            WaveManager.Instance.OnWaveBegin += OnWaveBegin;
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
            return $"<color=#63ab3f>When entering a new room or when a wave spawns while you are in the arena, gain {GameTools.WrapColor($"{RegenMultiplier:N1}x", Color.yellow)} " +
                   $"health regen and {GameTools.WrapColor($"{DamageMultiplier:P0}", Color.yellow)} bonus damage for {GameTools.WrapColor($"{_duration} seconds", Color.white)}.</color>";
        }
    }
}
