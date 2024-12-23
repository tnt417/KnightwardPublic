using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;

namespace TonyDev.Game.Core.Effects
{
    public class AshenIdolEffect : GameEffect
    {
        public float DamageBonusPerEnemy;
        public int MaxEnemies;
        public float UpdateTimer;
        public float Range;

        private float _timer;
        
        public override void OnAddOwner()
        {
            
        }

        public override void OnRemoveOwner()
        {
            
        }

        public override void OnUpdateOwner()
        {
            _timer += Time.deltaTime;

            if (!(_timer > UpdateTimer)) return;
            
            _timer = 0;
            var count = Mathf.Min(
                GameManager.GetEntitiesInRange(Entity.transform.position, Range).Count(ge => ge.Team == Team.Enemy),
                MaxEnemies);
                
            Entity.Stats.RemoveBuffsOfSource("AshenIdol");
            Entity.Stats.AddBuff(new StatBonus(StatType.AdditivePercent, Stat.Damage, DamageBonusPerEnemy * count, "AshenIdol"), UpdateTimer);
        }
        
        public override string GetEffectDescription()
        {
            return
                $"<color=#63ab3f>For every nearby enemy, gain <color=yellow>{DamageBonusPerEnemy:P0}</color> extra damage, up to a maximum of <color=yellow>{MaxEnemies*DamageBonusPerEnemy:P0}</color>.</color>";
        }
    }
}