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

        private float _durabilityTimer = 0f;
        
        private void FixedUpdate()
        {
            if (tower.Targets.Count == 0 || tower.Targets[0] == null || tower.Stats.GetStat(Stat.AttackSpeed) == 0)
            {
                tower.UpdateTargets();
                if (tower.Targets.Count == 0 || tower.Targets[0] == null || tower.Stats.GetStat(Stat.AttackSpeed) == 0)
                {
                    particles.Stop();
                    flameCollider.enabled = false;
                    return;
                }
            }

            if (!particles.isPlaying)
            {
                particles.Play();
                flameCollider.enabled = true;
            }

            _durabilityTimer += Time.fixedDeltaTime;

            if (_durabilityTimer > 1)
            {
                tower.SubtractDurability(1);
                _durabilityTimer = 0;
            }

            attackComponent.damageCooldown = 1f / tower.Stats.GetStat(Stat.AttackSpeed);

            head.transform.right = (head.transform.position - tower.Targets[0].transform.position).normalized;
        }
    }
}
