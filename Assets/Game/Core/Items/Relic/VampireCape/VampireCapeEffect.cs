using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Items.Relics.VampireCape
{
    public class VampireCapeEffect : GameEffect
    {
        private float _batRange = 5f;
        private float _lifestealAmount = 0.1f;
        
        public override void OnAddOwner()
        {
            //Entity.OnDamageOther += LifeSteal;
        }

        public override void OnRemoveOwner()
        {
            //Entity.OnDamageOther -= LifeSteal;
        }

        private void LifeSteal(float amount, GameEntity other)
        {
            var lifesteal = amount * _lifestealAmount;
            
            Entity.ApplyDamage(-(lifesteal));
            ObjectSpawner.SpawnTextPopup(Entity.transform.position, "+" + lifesteal, Color.green);
        }

        private double _nextUpdateTime;

        public override void OnUpdateOwner() //Only update every half second to prevent excessive lag
        {
            if (Time.time < _nextUpdateTime) return;

            _nextUpdateTime = Time.time + 0.1f;

            foreach (var ge in GameManager.EntitiesReadonly.Where(ge => ge is Enemy {EnemyName: "Bat"}))
            {
                if (Vector2.Distance(ge.transform.position, Entity.transform.position) < _batRange)
                {
                    ge.SetTeam(Entity.Team, Entity.Team == Team.Enemy ? Team.Player : Team.Enemy);
                }
            }
        }
    }
}
