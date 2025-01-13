using System;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Crystal
{
    public class Crystal : GameEntity
    {
        public static Crystal Instance;
        
        private new void Awake()
        {
            if(Instance == null) Instance = this;
            base.Awake();
        }

        private void Start()
        {
            Init();

            if (!EntityOwnership) return;
            
            //CmdAddEffect(new CrystalArmorEffect(), this);
        }

        public Action<bool> OnVisibilityChange;
        
        private void OnBecameVisible()
        {
            OnVisibilityChange?.Invoke(true);
        }
        
        private void OnBecameInvisible()
        {
            OnVisibilityChange?.Invoke(false);
        }

        [Command(requiresAuthority = false)]
        private void CmdCrystalDie()
        {
            RpcCrystalDie();
        }
        
        [ClientRpc]
        private void RpcCrystalDie()
        {
            GetComponent<Animator>().Play("CrystalExplode");
        }

        private bool _died = false;
        
        //Interface code. Only abnormal thing is the game is over when the crystal dies.
        #region IDamageable
        public override Team Team => Team.Player;
        public override void Die()
        {
            if (!EntityOwnership || _died) return;

            _died = true;
            
            TriggerDeathOwner();
            CmdCrystalDie();
        }
        #endregion

        [GameCommand(Keyword = "ci", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Toggled crystal invulnerability.")]
        public static void ToggleInvulnerable()
        {
                var crystal = FindObjectOfType<Crystal>();
                Instance.CmdSetInvulnerable(!Instance.IsInvulnerable);
        }
    }
}
