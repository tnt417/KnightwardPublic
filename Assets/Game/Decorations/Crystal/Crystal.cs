using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Crystal : MonoBehaviour, IDamageable
{
    public void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    
    //Interface code. Only abnormal thing is the game is over when the crystal dies.
    #region IDamageable

    public Team team => Team.Player;
    public int MaxHealth => 1000;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Timer.Stop();
        SceneManager.LoadScene("Scenes/GameOver");
    }

    public event IDamageable.HealthAction OnHealthChanged;
    #endregion
}
