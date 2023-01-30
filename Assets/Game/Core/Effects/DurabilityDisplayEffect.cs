using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Towers;
using UnityEngine;

namespace TonyDev
{
    public class DurabilityDisplayEffect : GameEffect
    {
        private Tower Tower => Entity == null ? null : Entity as Tower;

        public override string GetEffectDescription()
        {
            if (Item == null) return "";

            var currentDurability = (int) Item.statBonuses.Where(sb => sb.stat == Stat.Health).Sum(sb => sb.strength);
            var maxDurability = (int) Item.statBonuses
                .Where(sb => sb.stat == Stat.Health && sb.source != Tower.DurabilityNegationSource)
                .Sum(sb => sb.strength);
            
            return (currentDurability <= 0 ? "<color=red>" : "<color=yellow>") + "<size=18>" + (maxDurability >= 1000000 ? "Infinite" : currentDurability) +
                   "</size></color>" + "<color=grey><size=15>" + (maxDurability >= 1000000 ? "" : "/" + maxDurability) + "</size></color><color=grey><size=18> durability</size></color>";
        }
    }
}