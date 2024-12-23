using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class TitansAegis : GameEffect
    {
        public float InvulnTimer;
        public float RepelRange;
        public float KnockbackForce;

        private float _timer;
        private bool _active = false;
    
        public override void OnAddOwner()
        {
            Entity.OnTryHurtInvulnerableOwner += TryDeflect;
        }

        public override void OnRemoveOwner()
        {
            Entity.OnTryHurtInvulnerableOwner -= TryDeflect;
        }

        public override void OnUpdateOwner()
        {
            if (Entity.IsInvulnerable) return;
            
            _timer += Time.deltaTime;

            if (_timer > InvulnTimer)
            {
                _timer = 0;
                Entity.IsInvulnerable = true;
            }
        }
        
        private void TryDeflect(float damage)
        {
            var pos = Entity.transform.position;
            
            ObjectSpawner.SpawnTextPopup(pos, "Blocked!", Color.blue, 0.5f);

            SoundManager.PlaySound("anvil",0.5f, pos);
            
            foreach(var ge in GameManager.GetEntitiesInRange(Entity.transform.position, RepelRange).Where(ge => ge.Team == Team.Enemy))
            {
                ge.ApplyKnockbackGlobal((ge.transform.position - Entity.transform.position).normalized * KnockbackForce);
            }

            Entity.IsInvulnerable = false;
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Every <color=yellow>{InvulnTimer:N0}</color> seconds, block the next hit. Upon blocking, knock back nearby enemies.</color>";
        }
    }
}