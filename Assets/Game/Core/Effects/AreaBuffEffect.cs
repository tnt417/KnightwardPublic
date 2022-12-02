using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class AreaBuffEffect : GameEffect
    {
        public StatBonus[] StatBonuses;
        public Team targetTeam;
        public float Radius;

        private float _nextUpdateTime;

        public override void OnUpdateClient()
        {
            if (Time.time < _nextUpdateTime) return;
            _nextUpdateTime = Time.time + 0.1f;

            foreach (var t in GameManager.EntitiesReadonly.Where(e =>
                e.Team == targetTeam && e.EntityOwnership &&
                Vector2.Distance(Entity.transform.position, e.transform.position) < Radius))
            {
                foreach (var sb in StatBonuses)
                {
                    t.Stats.AddBuff(sb, 0.1f);
                }
            }
        }
    }
}