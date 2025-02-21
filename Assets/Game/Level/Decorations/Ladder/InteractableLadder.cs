using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Ladder
{
    public class InteractableLadder : Interactable
    {
        [SerializeField] private bool regen = true;
        [SerializeField] private bool winGame = false;

        protected override void OnInteract(InteractType type)
        {
            if (Player.LocalInstance.playerAnimator.isInLadderAnim) return;
            
            if (winGame || (GameManager.IsDemo && GameManager.DungeonFloor == 5 && regen))
            {
                Player.LocalInstance.playerAnimator.JumpIntoLadderTask(gameObject, regen).Forget();
                GameManager.Instance.GameWin();
                return;
            }
            
            PlayInteractSound();

            InteractTask().Forget();
        }

        private async UniTask InteractTask()
        {
            await Player.LocalInstance.playerAnimator.JumpIntoLadderTask(gameObject, regen);
            
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
