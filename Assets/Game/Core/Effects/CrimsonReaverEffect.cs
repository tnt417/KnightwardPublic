using TonyDev.Game.Core.Entities;

namespace TonyDev.Game.Core.Effects
{
    public class CrimsonReaverEffect : GameEffect
    {
        public float PercentMissingHealth;
        public float DamageMultiplier;

        private int _count;

        public override void OnAddOwner()
        {
            Entity.OnDamageOther += (dmg, entity, crit, type) =>
            {
                if(dmg >= Entity.Stats.GetStat(Stat.Damage)/2 && type != DamageType.Absolute) TriggerEffect(dmg, entity);
            };
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= (dmg, entity, crit, type) =>
            {
                if(dmg >= Entity.Stats.GetStat(Stat.Damage)/2 && type != DamageType.Absolute) TriggerEffect(dmg, entity);
            };
        }

        private void TriggerEffect(float dmg, GameEntity other)
        {
            _count++;

            if (_count < 3) return;

            _count = 0;
            
            other.CmdDamageEntity(dmg * DamageMultiplier, true, null, true, DamageType.Absolute );
            Entity.CmdDamageEntity((Entity.Stats.GetStat(Stat.Health) - Entity.CurrentHealth) * -PercentMissingHealth, false, null, true, DamageType.Heal );
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Every <color=yellow>third</color> attack deals <color=yellow>{DamageMultiplier:N0}</color>x damage and restores <color=yellow>{PercentMissingHealth:P0}</color> of the player's missing health</color>";
        }
    }
}