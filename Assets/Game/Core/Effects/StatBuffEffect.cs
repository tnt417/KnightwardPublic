using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev
{
    public class StatBuffEffect : GameEffect
    {
        public StatBonus[] StatBonuses;
        public float Duration;

        public override void OnAddOwner()
        {
            foreach (var sb in StatBonuses)
            {
                Entity.Stats.AddBuff(sb, Duration);   
            }
        }
    }
}
