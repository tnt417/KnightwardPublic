using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;

namespace TonyDev.Game.Core.Effects.ItemEffects
{
    [GameEffect(ID = "crystalNecklaceEffect")]
    public class CrystalNecklaceEffect : GameEffect
    {
        private const float Multiplier = 0.4f; //The portion of incoming player damage to redirect to the crystal

        public override void OnAdd(GameEntity source) //Runs when an item is equipped
        {
            Crystal.CrystalRegen = () => (1f + PlayerStats.Stats.GetStat(Stat.HpRegen)) * 10f; //Sets the crystal regen to be 10x the player's regen
            PlayerStats.Stats.AddStatBonus(StatType.Flat, Stat.Armor, 50f, "CrystalNecklace"); //Give the player 50 armor
            Player.Instance.OnHurt += DamageCrystal; //When the player is damaged, call our DamageCrystal method
        }

        public override void OnRemove() //Runs when an item is unequipped
        {
            Crystal.CrystalRegen = () => 0; //Sets the crystal regen back to 0
            PlayerStats.Stats.RemoveStatBonuses("CrystalNecklace");
            Player.Instance.OnHurt -= DamageCrystal; //Stop calling DamageCrystal
        }

        public override void OnUpdate() {}

        private void DamageCrystal(float damage)
        {
            GameManager.CrystalHealth -= damage * Multiplier * 10; //Damage the crystal for 40% of incoming player damage times 10
        }
    }
}
