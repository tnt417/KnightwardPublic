using System.Linq;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;

namespace TonyDev.Game.Core.Effects
{
    public class PerfectedStrikeEffect : GameEffect
    {
        public float remainingCooldownReduce;
        
        public override void OnAddOwner()
        {
            Entity.OnDamageOther += HitEffect;
        }

        public override void OnRemoveOwner()
        {
            Entity.OnDamageOther -= HitEffect;
        }

        private void HitEffect(float damage, GameEntity other, bool isCrit, DamageType dt)
        {
            if (!isCrit) return;
            
            if (Entity is Player)
            {
                ObjectSpawner.SpawnTextPopup(other.transform.position, "Cooldown discounted!", Color.cyan);
                foreach (var ability in PlayerInventory.Instance.WeaponItem.itemEffects.OfType<AbilityEffect>())
                {
                    ability.DiscountCooldown(remainingCooldownReduce, false);
                }
            }
        }
    }
}