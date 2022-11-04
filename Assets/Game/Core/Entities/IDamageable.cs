using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Global;

namespace TonyDev.Game.Core.Entities
{
    public interface IDamageable
    {
        Team Team { get; }
        float DamageMultiplier { get; }
        float HealMultiplier { get; }
        int MaxHealth { get; }
        float CurrentHealth { get; }
        float ApplyDamage(float damage, out bool successful, bool ignoreInvincibility = false);
        bool IsInvulnerable { get; }
        bool IsTangible { get; }
        void Die();
        delegate void HealthAction(float value);
        event HealthAction OnHealthChangedOwner;
        event HealthAction OnHealOwner;
        event HealthAction OnHurtOwner;
        event HealthAction OnDeathOwner;
    }
}
