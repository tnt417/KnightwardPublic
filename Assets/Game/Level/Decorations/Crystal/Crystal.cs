using System;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Crystal
{
    public class Crystal : GameEntity
    {

        public static Func<float> CrystalRegen = () => 0;

        private void Update()
        {
            var hpRegen = CrystalRegen.Invoke() * Time.deltaTime;
            ApplyDamage(-hpRegen);
        }
        
        //Interface code. Only abnormal thing is the game is over when the crystal dies.
        #region IDamageable
        public override Team Team => Team.Player;
        public override int MaxHealth => 1000;
        public override float CurrentHealth {
            get => GameManager.CrystalHealth;
            protected set => GameManager.CrystalHealth = value;
        }

        #endregion

        [GameCommand(Keyword = "ci", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Toggled crystal invulnerability.")]
        public static void ToggleInvulnerable()
        {
                var crystal = FindObjectOfType<Crystal>();
                crystal.IsInvulnerable = !crystal.IsInvulnerable;
        }
    }
}
