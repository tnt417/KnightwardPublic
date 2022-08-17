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
        float ApplyDamage(float damage);
        bool IsInvulnerable { get; }
        void Die();
        delegate void HealthAction(float value);
        event HealthAction OnHealthChanged;
        event HealthAction OnHeal;
        event HealthAction OnHurt;
        event HealthAction OnDeath;
    }
}
