using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PoisonInflictEffect : GameEffect
    {
        public float TotalDamageMultiplier;
        public float TickFrequency;
        public int TickAmount;
        
        public override void OnAddOwner()
        {
            Entity.OnDamageOther += InflictPoison;
        }

        public void InflictPoison(float dmg, GameEntity entity, bool crit)
        {
            entity.CmdAddEffect(new PoisonEffect()
            {
                Damage = TotalDamageMultiplier * Entity.Stats.GetStat(Stat.Damage) / TickAmount,
                Ticks = TickAmount,
                Frequency = TickFrequency
            }, Entity);
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= InflictPoison;
        }
    }
}