using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public class EnemyMovementChase : EnemyMoveBase
    {
        //Editor variables
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private float speedMultiplier;
        //
    
        public override void UpdateMovement()
        {
            //Move towards the target
            rb2D.transform.Translate((Target.position - transform.position).normalized * speedMultiplier * Time.fixedDeltaTime);
        }
    }
}
