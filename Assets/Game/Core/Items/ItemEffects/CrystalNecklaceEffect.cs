using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;

namespace TonyDev.Game.Core.Items.ItemEffects
{
    [CreateAssetMenu(menuName = "Item Effects/Crystal Necklace Effect")]
    public class CrystalNecklaceEffect : ItemEffect
    {
        private const float Multiplier = 0.4f; //The portion of incoming player damage to redirect to the crystal

        public override void OnAdd() //Runs when an item is equipped
        {
            Crystal.CrystalRegen = () => (1f + PlayerStats.GetStatBonus(Stat.HpRegen)) * 10f; //Sets the crystal regen to be the player's regen
            PlayerStats.HpRegenHandler = () => 0; //Set the players regen to be nothing
            PlayerStats.DamageReductionHandler = () => PlayerStats.GetStatBonus(Stat.DamageReduction) + Multiplier; //Give the player 20% damage reduction
            Player.OnPlayerDamage += DamageCrystal; //When the player is damaged, call our DamageCrystal method
        }

        public override void OnRemove() //Runs when an item is unequipped
        {
            Crystal.CrystalRegen = () => 0; //Sets the crystal regen back to 0
            PlayerStats.HpRegenHandler = () => 1f + PlayerStats.GetStatBonus(Stat.HpRegen); //Sets the player regen back to what it normally is
            PlayerStats.DamageReductionHandler = () => PlayerStats.GetStatBonus(Stat.DamageReduction); //Return the player's damage reduction to normal
            Player.OnPlayerDamage -= DamageCrystal; //Stop calling DamageCrystal
        }

        public override void OnUpdate() {}

        private void DamageCrystal(float damage)
        {
            GameManager.CrystalHealth -= damage * Multiplier; //Damage the crystal for 20% of incoming player damage
        }
    }
}
