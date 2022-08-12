using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using UnityEngine;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "spearEffect")]
    public class SpearEffect : AbilityEffect
    {
        public override void OnAdd(GameEntity source)
        {
            ActivateButton = KeyCode.Q;
            Cooldown = 10f;
            Duration = 5f;
        }

        public override void OnRemove()
        {
            OnAbilityDeactivate();
        }

        protected override void OnAbilityActivate()
        {
            PlayerInventory.Instance.WeaponItem.projectiles[0].effectIDs.Add("poison");
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.MoveSpeed, 1.4f, "spearEffect"), Duration);
            Entity.Stats.AddBuff(new StatBonus(StatType.Multiplicative, Stat.AttackSpeed, 1.4f, "spearEffect"), Duration);
        }

        protected override void OnAbilityDeactivate()
        {
            PlayerInventory.Instance.WeaponItem.projectiles[0].effectIDs.RemoveAll(id => id == "poison");
        }
    }
}
