using System.Linq;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Towers
{
    public class ProjectileTower : Tower
    {
        //Editor variables
        [SerializeField] private GameObject rotateToFaceTargetObject; //Rotates to face direction of firing
        [SerializeField] private ProjectileData[] projectileData; //Prefab spawned upon fire
        //

        public bool targetFullHp = true;

        public string fireSound;

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            OnAttack += CmdFire;
        }

        [Command(requiresAuthority = false)]
        private void CmdFire()
        {
            var rpcSent = false;
            
            foreach (var direction in Targets.Where(t => t != null && (targetFullHp || !Mathf.Approximately(t.CurrentHealth, t.MaxHealth))).Select(t =>
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
            if(!string.IsNullOrEmpty(fireSound)) SoundManager.PlaySound(fireSound,0.2f, transform.position, Random.Range(0.95f, 1.05f));
            towerAnimator.PlayAnimation(TowerAnimationState.Fire);
            if (rotateToFaceTargetObject != null) rotateToFaceTargetObject.transform.right = direction;
        }
    }
}