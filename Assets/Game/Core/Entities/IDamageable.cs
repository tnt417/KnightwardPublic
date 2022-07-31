namespace TonyDev.Game.Core.Combat
{
    public interface IDamageable
    {
        Team Team { get; }
        float DamageMultiplier { get; }
        float HealMultiplier { get; }
        int MaxHealth { get; }
        float CurrentHealth { get; }
        void ApplyDamage(float damage);
        bool IsInvulnerable { get; }
        void Die();
        delegate void HealthAction();
        event HealthAction OnHealthChanged;
        event HealthAction OnHeal;
        event HealthAction OnHurt;
        event HealthAction OnDeath;
    }
}
