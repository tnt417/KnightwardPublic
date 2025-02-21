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

        public override void OnAddClient()
        {
            //Entity.OnDeathBroadcast += Die;
        }

        private void Die()
        {
            if (Entity == null) return;
            
            var prefab = ObjectFinder.GetPrefab("ExplosiveDeath");

            var go = Object.Instantiate(prefab, Entity.transform.position, Quaternion.identity);

            go.GetComponent<AttackComponent>().damage = Entity.Stats.GetStat(Stat.Damage);
        }
    }
}
