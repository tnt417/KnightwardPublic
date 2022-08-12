using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class ProjectileTower : Tower
    {
        //Editor variables
        [SerializeField] private GameObject rotateToFaceTargetObject; //Rotates to face direction of firing
        [SerializeField] private ProjectileData[] projectileData; //Prefab spawned upon fire
        //

        private new void Start()
        {
            base.Start();

            OnAttack += () =>
            {
                towerAnimator.PlayAnimation(TowerAnimationState.Fire);

                foreach (var direction in Targets.Where(t => t != null).Select(t =>
                    (t.transform.position - transform.position).normalized))
                {
                    foreach (var projData in projectileData)
                    {
                        if (rotateToFaceTargetObject != null) rotateToFaceTargetObject.transform.right = direction;

                        AttackFactory.CreateProjectileAttack(this, direction, projData);
                    }
                }
            };
        }
    }
}