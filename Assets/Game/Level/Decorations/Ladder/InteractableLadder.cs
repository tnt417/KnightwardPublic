using System;
using System.Collections;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Decorations.Ladder
{
    public class InteractableLadder : Interactable
    {
        [SerializeField] private bool regen = true;

        protected override void OnInteract()
        {
            if(!regen) GameManager.Instance.TogglePhase();
            else Regen();
        }

        [GameCommand(Keyword = "regen", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Success!")]
        public static void Regen()
        {
            GameManager.Instance.CmdProgressNextDungeonFloor();
        }
    }
}
