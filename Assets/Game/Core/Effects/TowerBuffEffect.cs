using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class TowerBuffEffect : GameEffect
    {
        public float AttackSpeedBonus;
        public float DamageBonus;
        public float Radius;

        private float _nextUpdateTime;
        
        public override void OnAddOwner()
        {
            
        }

        public override void OnRemoveOwner()
        {
            
        }

        public override void OnUpdateOwner()
        {
            if (Time.time < _nextUpdateTime) return;
            _nextUpdateTime = Time.time + 0.1f;

            foreach (var t in GameManager.EntitiesReadonly.Where(e => e is Tower && Vector2.Distance(Entity.transform.position, e.transform.position) < Radius))
            {
                t.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.AttackSpeed, AttackSpeedBonus, Entity.name),0.1f);
                t.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, DamageBonus, Entity.name),0.1f);
            }
        }
    }
}