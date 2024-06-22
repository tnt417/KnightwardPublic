using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.HealChalice
{
    public class HealChaliceInteractable : Interactable
    {
        private int _crystalHeal = 500;
        protected override void OnInteract(InteractType type)
        {
            ObjectSpawner.SpawnTextPopup(transform.position, "Crystal and player healed!", Color.green, 0.4f);
        }

        [Server]
        public void OnInteractServer()
        {
            Crystal.Crystal.Instance.CmdDamageEntity(-_crystalHeal, false, null, true, DamageType.Heal);
        }

        public void OnInteractGlobal()
        {
            isInteractable = false;
            Player.LocalInstance.FullHeal();
        }
    }
}
