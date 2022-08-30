using System;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class Tower : GameEntity
    {
        //Editor variables
        [SerializeField] protected TowerAnimator towerAnimator;
        [SerializeField] public float targetRadius;
        //

        private void Start()
        {
            Init();
        }

        public override Team Team => Team.Player;
        public override bool IsInvulnerable => true;
    }
}