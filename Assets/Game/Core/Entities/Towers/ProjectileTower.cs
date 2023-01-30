using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Enemies.Attack;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class ProjectileTower : Tower
    {
        //Editor variables
        [SerializeField] private GameObject rotateToFaceTargetObject; //Rotates to face direction of firing
        [SerializeField] private ProjectileData[] projectileData; //Prefab spawned upon fire
        //

        public string fireSound;

        private void Start()
        {
            base.Start();
            
            if (!EntityOwnership) return;

            OnAttack += CmdFire;
        }

        [Command(requiresAuthority = false)]
        private void CmdFire()
        {
            var rpcSent = false;
            
            foreach (var direction in Targets.Where(t => t != null).Select(t =>
                (t.transform.position - transform.position).normalized))
            {
                foreach (var projData in projectileData)
                {
                    ObjectSpawner.SpawnProjectile(this, transform.position, direction, projData);
                }

                if (!rpcSent)
                {
                    SubtractDurability(1);
                    RpcFire(direction);
                }
                rpcSent = true;
            }
        }

        [ClientRpc]
        private void RpcFire(Vector2 direction)
        {
            if(!string.IsNullOrEmpty(fireSound)) SoundManager.PlaySound(fireSound,0.5f, transform.position);
            towerAnimator.PlayAnimation(TowerAnimationState.Fire);
            if (rotateToFaceTargetObject != null) rotateToFaceTargetObject.transform.right = direction;
        }
    }
}