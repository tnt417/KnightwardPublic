using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PredatoryMark : GameEffect
    {
        public float ExpireTime;
        public float BonusDuration;
        public float BonusDamagePerEnemy;
        public int MaximumEnemies;

        private float _expireTimer;

        public override void OnAddOwner()
        {
            Entity.OnDamageOther += (dmg, entity, crit, type) =>
            {
                if(entity == null || !entity.IsAlive) DoBuff();
            };
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= (dmg, entity, crit, type) =>
            {
                if(entity == null || !entity.IsAlive) DoBuff();
            };
        }
        
        public override void OnUpdateOwner()
        {
            _expireTimer += Time.deltaTime;

            if (_expireTimer >= ExpireTime)
            {
                _expireTimer = 0f;
                
                ResetBonuses();
            }
        }

        private void ResetBonuses()
        {
            Entity.Stats.RemoveBuffsOfSource("PredatoryMark");
        }
        
        private void DoBuff()
        {
            var sbs= Entity.Stats.GetStatBonuses(Stat.Damage, true);
            
            _expireTimer = 0f;
            
            // Get the nth newest stat bonus and remove it to make way for the new one
            var oldestMaxSb = sbs.Where(s => s.source.StartsWith("PredatoryMark_BUFF"))
                .OrderBy(s => s.source.Split("_BUFF").Last()).ElementAtOrDefault(MaximumEnemies-1);

            if(!string.IsNullOrEmpty(oldestMaxSb.source)) Entity.Stats.RemoveBuffs(new HashSet<string>{ oldestMaxSb.source });
            
            Entity.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, BonusDamagePerEnemy, "PredatoryMark"), BonusDuration);
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>Gain <color=yellow>{BonusDamagePerEnemy:P0}</color> damage for each enemy you've slain in the last <color=yellow>{BonusDuration:N0}</color> seconds, resetting if no enemies are killed within <color=yellow>{ExpireTime:N0}</color> seconds. Maximum of <color=yellow>{BonusDamagePerEnemy*MaximumEnemies:P0}</color> damage.</color>";
        }
    }
}