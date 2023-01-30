using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class SacrificeInteractable : Interactable
    {
        protected override void OnInteract(InteractType type)
        {
            if (PlayerInventory.Instance.RelicItems.Count == 0)
            {
                PlayInteractSound();

                ObjectSpawner.SpawnTextPopup(transform.position,
                    "<color=red>You do not have any relics to offer...</color>", Color.red, 0.3f);
                
                return;
            }
            
            var scramble = Random.Range(0, 3);
            
            for (var i = 0; i <= scramble; i++)
            {
                var takenItem = PlayerInventory.Instance.RelicItems.Dequeue();
                if (i != scramble)
                {
                    PlayerInventory.Instance.RelicItems.Enqueue(takenItem);
                }
                else
                {
                    PlayerInventory.Instance.OnUnEquip(takenItem);
                }
            }

            var boostStat = Enum.Parse<Stat>(GameTools.SelectRandom(Enum.GetNames(typeof(Stat))));
            ObjectSpawner.SpawnTextPopup(transform.position, "<color=green>+40% " + PlayerStats.StatLabelKey[boostStat] + "</color>, <color=red>-1 relic.</color>", Color.green, 0.4f);
            Player.LocalInstance.Stats.AddStatBonus(StatType.AdditivePercent, boostStat, 0.4f, "Chalice");
            IsInteractable = false;
            PlayInteractSound();
        }
    }
}
