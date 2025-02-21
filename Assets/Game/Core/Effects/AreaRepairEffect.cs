using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev
{
    public class AreaRepairEffect : GameEffect
    {
        public float Percent;
        public float Cooldown;
        public float Range;

        private float _timer;

        public override void OnUpdateServer()
        {
            _timer += Time.deltaTime;

            if (_timer < Cooldown) return;

            _timer = 0;

            foreach (var ge in GameManager.GetEntitiesInRange(Entity.transform.position, Range)
                .Where(ge => ge is Tower {MaxDurability: < 1000000} t && t.durability != t.MaxDurability))
            {
                if (ge is Tower t)
                {
                    t.SubtractDurability((int) -Mathf.Clamp(t.MaxDurability * Percent, 0,
                        t.MaxDurability - t.durability));
                }
            }
        }
    }
}