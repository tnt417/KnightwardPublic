using TonyDev.Game.Core.Effects.ItemEffects;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.UI.Popups;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    [GameEffect(ID = "poison")]
    public class PoisonEffect : GameEffect
    {
        public int Ticks = 10;
        public float Frequency = 0.5f;
        public float Damage = 1f;

        private float _timer;
        
        public override void OnAdd(GameEntity source)
        {
            Damage = source.Stats.GetStat(Stat.Damage) / 10f;
        }

        public override void OnRemove()
        {
            
        }

        public override void OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_timer > Frequency)
            {
                Entity.ApplyDamage(Damage);
                PopupManager.SpawnPopup(Entity.transform.position, (int)Damage, false);
                _timer = 0f;
                
                Ticks--;
                
                if (Ticks <= 0)
                {
                    Entity.RemoveEffect(this);
                }
            }
        }
    }
}
