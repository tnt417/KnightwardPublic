using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    [ItemEffect(ID = "crystalNecklaceEffect")]
    public class CrystalNecklaceEffect : ItemEffect
    {
        private const float Multiplier = 0.4f; //The portion of incoming player damage to redirect to the crystal

        public override void OnAdd() //Runs when an item is equipped
        {
            Crystal.CrystalRegen = () => (1f + PlayerStats.GetStat(Stat.HpRegen)) * 10f; //Sets the crystal regen to be 10x the player's regen
            PlayerStats.AddStatBonus(StatType.Flat, Stat.Armor, 50f, "CrystalNecklace"); //Give the player 50 armor
            Player.OnPlayerDamage += DamageCrystal; //When the player is damaged, call our DamageCrystal method
        }

        public override void OnRemove() //Runs when an item is unequipped
        {
            Crystal.CrystalRegen = () => 0; //Sets the crystal regen back to 0
            PlayerStats.RemoveStatBonuses("CrystalNecklace");
            Player.OnPlayerDamage -= DamageCrystal; //Stop calling DamageCrystal
        }

        public override void OnUpdate() {}

        private void DamageCrystal(float damage)
        {
            GameManager.CrystalHealth -= damage * Multiplier * 10; //Damage the crystal for 40% of incoming player damage times 10
        }
    }
}
