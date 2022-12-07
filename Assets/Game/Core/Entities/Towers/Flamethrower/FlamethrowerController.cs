using System;
using Mirror;
using TonyDev.Game.Core.Attacks;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Towers.Flamethrower
{
    public class FlamethrowerController : NetworkBehaviour
    {
        public Tower tower;
        public GameObject head;
        public ParticleSystem particles;
        public AttackComponent attackComponent;
        public Collider2D flameCollider;

        public override void OnStartServer()
        {
            attackComponent.SetData(null, tower);
        }

        private void FixedUpdate()
        {
            if (tower.Targets.Count == 0 || tower.Targets[0] == null)
            {
                tower.UpdateTarget();
                particles.Stop();
                flameCollider.enabled = false;
                return;
            }
            
            if (!particles.isPlaying)
            {
                particles.Play();
                flameCollider.enabled = true;
            }

            attackComponent.damageCooldown = 1f / tower.Stats.GetStat(Stat.AttackSpeed);

            head.transform.right = (head.transform.position - tower.Targets[0].transform.position).normalized;
        }
    }
}
