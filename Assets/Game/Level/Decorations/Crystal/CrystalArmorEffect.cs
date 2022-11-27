using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Level.Decorations.Crystal
{
    public class CrystalArmorEffect : GameEffect
    {
        private float _nextBuffTime;
        
        public override void OnUpdateOwner()
        {
            if (Time.time < _nextBuffTime) return;
            
            _nextBuffTime = Time.time + 1f;
            
            Entity.Stats.AddBuff(new StatBonus(StatType.Flat, Stat.Armor, GameManager.EnemyDifficultyScale * 20f, EffectIdentifier), 1f);
        }
    }
}
