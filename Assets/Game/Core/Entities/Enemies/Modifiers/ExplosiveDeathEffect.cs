using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Modifiers
{
    public class ExplosiveDeathEffect : GameEffect
    {
        public float explodeRadius;
        public float explodeHealthProportion;

        public override void OnAddServer()
        {
            Entity.OnDeath += Die;
        }

        private void Die(float value)
        {
            if (Entity == null) return;
            
            var prefab = ObjectFinder.GetPrefab("ExplosiveDeath");

            var go = Object.Instantiate(prefab, Entity.transform.position, Quaternion.identity);

            NetworkServer.Spawn(go);
        }
    }
}
