using System;
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
        
        public static Func<float> CrystalRegen = () => 0;

        private new void Awake()
        {
            if(Instance == null) Instance = this;
            base.Awake();
        }

        private void Start()
        {
            Init();
        }

        private new void Update()
        {
            var hpRegen = CrystalRegen.Invoke() * Time.deltaTime;
            ApplyDamage(-hpRegen);
        }
        
        //Interface code. Only abnormal thing is the game is over when the crystal dies.
        #region IDamageable
        public override Team Team => Team.Player;
        public override int MaxHealth => 1000;
        #endregion

        [GameCommand(Keyword = "ci", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Toggled crystal invulnerability.")]
        public static void ToggleInvulnerable()
        {
                var crystal = FindObjectOfType<Crystal>();
                crystal.IsInvulnerable = !crystal.IsInvulnerable;
        }
    }
}
