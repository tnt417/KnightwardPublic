using System;
using System.Collections;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private bool winGame = false;

        protected override void OnInteract(InteractType type)
        {
            if (Player.LocalInstance.playerAnimator.isInLadderAnim) return;
            
            if (winGame)
            {
                GameManager.Instance.GameWin();
                return;
            }
            
            PlayInteractSound();

            InteractTask().Forget();
        }

        private async UniTask InteractTask()
        {
            await Player.LocalInstance.playerAnimator.JumpIntoLadderTask(gameObject);
            
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
