namespace TonyDev.Game.Core.Combat
{
    public interface IDamageable
    {
        Team team { get; }
        int MaxHealth { get; }
        float CurrentHealth { get; }
        bool IsAlive { get; }
        void ApplyDamage(int damage);
        void Die();
        delegate void HealthAction();
        event HealthAction OnHealthChanged;
    }
}
