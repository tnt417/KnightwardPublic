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
        protected override void OnInteract(InteractType type)
        {
            var boostStat = Enum.Parse<Stat>(Tools.SelectRandom(Enum.GetNames(typeof(Stat))));
            var curseStat = Enum.Parse<Stat>(Tools.SelectRandom(Enum.GetNames(typeof(Stat)).Where(s => s != Enum.GetName(typeof(Stat), boostStat))));
            ObjectSpawner.SpawnTextPopup(transform.position, "<color=green>Double " + PlayerStats.StatLabelKey[boostStat] +"</color>, <color=red>halve " + PlayerStats.StatLabelKey[curseStat] + ".", Color.white, 0.2f);
            Player.LocalInstance.Stats.AddStatBonus(StatType.Multiplicative, boostStat, 2f, "Curse");
            Player.LocalInstance.Stats.AddStatBonus(StatType.Multiplicative, curseStat, 0.5f, "Curse");
            IsInteractable = false;
            PlayInteractSound();
        }
    }
}
