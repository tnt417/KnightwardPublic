using System.Collections.Generic;
using Mirror;
using Newtonsoft.Json;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Modifiers
{
    public class EnemyModifiers
    {
        private static float PercentEnemiesSpecial => 1f;//1.1f-1/(0.025f*Timer.GameTimer+1);

        private static List<GameEffect> _modifierEffects;

        static EnemyModifiers()
        {
            _modifierEffects = new List<GameEffect>();
            
            _modifierEffects.Add(new ExplosiveDeathEffect()
            {
                explodeHealthProportion = 0.2f,
                explodeRadius = 2f
            });
        }
        
        [ServerCallback]
        public static void ModifyEnemy(GameEntity enemy)
        {
            if (Random.Range(0, 1) > PercentEnemiesSpecial) return; //If it fails the roll, do nothing.
            
            //Otherwise add modifiers
            enemy.AddEffect(new ExplosiveDeathEffect()
            {
                explodeHealthProportion = 0.2f,
                explodeRadius = 2f
            }, enemy);
        }
    }
}
