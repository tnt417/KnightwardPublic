using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Movement
{
    public class EnemyMovementChase : EnemyMovementBase
    {
        private bool _hadTargetPreviously = false;
        
        private void Update()
        {
            var hadTargetNow = FirstTarget != null;
            
            if(hadTargetNow && !_hadTargetPreviously) OnStartMove?.Invoke();
            if(!hadTargetNow && _hadTargetPreviously) OnStopMove?.Invoke();
            
            _hadTargetPreviously = hadTargetNow;
        }
        
        public override void UpdateMovement()
        {
            //Move towards the target
            transform.Translate((FirstTarget.position - transform.position).normalized * SpeedMultiplier * Time.fixedDeltaTime);
        }

        public override void PopulateFromData(EnemyMovementData data)
        {
            EnemyMovementData = data;
            SpeedMultiplier = data.speedMultiplier;
        }

        public override event MoveAction OnStartMove;
        public override event MoveAction OnStopMove;
    }
}
