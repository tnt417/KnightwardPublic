using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    [RequireComponent(typeof(Enemy), typeof(EnemyAnimator))]
    public abstract class EnemyMoveBase : MonoBehaviour, IEnemyMovement
    {
        private Enemy _enemy; //The enemy attached to the movement script
        protected EnemyAnimator EnemyAnimator; //The enemy animator;
        public AnimationClip[] animations;
        private void Start() //Initialize components
        {
            _enemy = GetComponent<Enemy>();
            EnemyAnimator = GetComponent<EnemyAnimator>();
            _enemy.UpdateTarget();
        }
        private void FixedUpdate()
        {
            if(Target != null) UpdateMovement(); //Call UpdateMovement
        }

        public bool DoMovement { get; } = true;
        public abstract void UpdateMovement();

        public float SpeedMultiplier { get; } = 1;
        public Transform Target => _enemy.Target;
    }
}
