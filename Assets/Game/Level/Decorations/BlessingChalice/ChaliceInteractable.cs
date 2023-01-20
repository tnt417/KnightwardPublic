using System;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.BlessingChalice
{
    public class ChaliceInteractable : Interactable
    {
        protected override void OnInteract(InteractType type)
        {
            var boostStat = Enum.Parse<Stat>(Tools.SelectRandom(Enum.GetNames(typeof(Stat))));
            ObjectSpawner.SpawnTextPopup(transform.position, "+20% " + PlayerStats.StatLabelKey[boostStat], Color.green, 0.4f);
            Player.LocalInstance.Stats.AddStatBonus(StatType.AdditivePercent, boostStat, 0.2f, "Chalice");
            IsInteractable = false;
            PlayInteractSound();
        }
    }
}
