using System;
using System.Linq;
using TonyDev.Game.Core.Combat;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    [RequireComponent(typeof(IEnemyMovement))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        //Editor variables
        [SerializeField] private EnemyAnimator enemyAnimator;
        [SerializeField] private int maxHealth;
        [SerializeField] private int moneyReward;
        //

        //Contains all interface code for IDamageable
        #region IDamageable
        public Team team => Team.Enemy;
        public int MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        [NonSerialized] public Transform Target;
        public void ApplyDamage(int damage)
        {
            enemyAnimator.PlayAnimation(EnemyAnimationState.Hurt); //Play the hurt animation
            var damageMultiplier = Mathf.Clamp01(1f - (Mathf.Log10(GameManager.EnemyDifficultyScale) - 0.5f)); //Enemies essentially gain damage resist as the difficulty scales.
            CurrentHealth -= damage * damageMultiplier;
            OnHealthChanged?.Invoke();
        
        
            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            GameManager.Money += moneyReward;
            GameManager.Enemies.Remove(this);
            Destroy(gameObject);
        }
        public event IDamageable.HealthAction OnHealthChanged;
        #endregion
    
        public void Awake()
        {
            GameManager.Enemies.Add(this); //Add this enemy to the GameManager's enemy list.
        
            //Initialize health variables.
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
        }

        public GameObject UpdateTarget() //Updates enemy's target and returns it.
        {
            var go = FindObjectsOfType
                    <MonoBehaviour>()
                .Where(mb => (mb as IDamageable)?.team == Team.Player && ((IDamageable) mb).IsAlive)
                .OrderBy( mb => Vector2.Distance(mb.transform.position, transform.position))
                .FirstOrDefault()
                ?.gameObject; //Finds closest non-dead damageable object on the player team
        
            Target = go == null ? null : go.transform; //Update the Target variable
        
            return go; //Returns the game object
        }
    }
}
