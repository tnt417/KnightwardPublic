using System;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Crystal
{
    public class Crystal : MonoBehaviour, IDamageable
    {

        public static Func<float> CrystalRegen = () => 0;

        private void Update()
        {
            CurrentHealth += CrystalRegen.Invoke() * Time.deltaTime;
            OnHealthChanged?.Invoke();
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        }
        
        //Interface code. Only abnormal thing is the game is over when the crystal dies.
        #region IDamageable

        public Team team => Team.Player;
        public int MaxHealth => 1000;
        public float CurrentHealth {
            get => GameManager.CrystalHealth;
            private set => GameManager.CrystalHealth = value;
        }
        public bool IsAlive => CurrentHealth > 0;
        public void ApplyDamage(int damage)
        {
            Debug.Log(damage);
            CurrentHealth -= damage;
            OnHealthChanged?.Invoke();

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            //This is managed in the GameManager class
        }

        public event IDamageable.HealthAction OnHealthChanged;
        #endregion
    }
}
