using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.UI;
using UnityEngine;

namespace TonyDev.Game.Core.Items
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance;

        //5 item variables, 1 for each inventory slot. A bit inefficient but works for now.
        public Item WeaponItem { get; private set; }
        public Item ArmorItem { get; private set; }

        public Queue<Item> RelicItems { get; private set; } = new Queue<Item>();

        private void Start()
        {
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Weapon,
                ItemRarity.Common)); //Add a starting sword to the player's inventory.
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Armor,
                ItemRarity.Common)); //Add armor to inventory
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Uncommon));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Rare));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Unique));
        }

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        }

        private void Update()
        {
            foreach (var r in RelicItems)
            foreach (var ie in r.itemEffects)
                ie.OnUpdate(); //Call OnUpdate for every item effect on the equipped relics
        }

        public void InsertTower(Item item) //Inserts a tower into the inventory and adds it to the UI
        {
            if (!item.IsSpawnable) return;
            TowerUIController.Instance.AddTower(item);
        }

        //Replaces/inserts items into inventory and returns the item that was replaced, if any.
        public Item InsertItem(Item item)
        {
            if (item == null) return null;
            if (item.itemType == ItemType.Tower)
            {
                InsertTower(item); //If it's a tower, insert it
                return null;
            }

            var replacedItem = SwapSlot(item); //Holds the replaced item to be returned at the end.

            if (item.IsEquippable)
            {
                if (replacedItem != null)
                {
                    ItemEffectManager.OnEffectsRemoved(replacedItem.itemEffects);
                    PlayerStats.RemoveStatBonuses(replacedItem.itemName); //Remove stat bonuses of the now removed item
                }

                ItemEffectManager.OnEffectsAdded(item.itemEffects);
                PlayerStats.AddStatBonusesFromItem(item); //Apply stat bonuses of the new item
            }

            return replacedItem; //Return the item that was replaced. If nothing was replaced, returns null.
        }

        private Item SwapSlot(Item item)
        {
            Item replacedItem = null; //Holds the replaced item to be returned at the end.
            switch (item.itemType)
            {
                case ItemType.Weapon: //If it's a weapon, it goes in the weapon slot.
                    replacedItem = WeaponItem;
                    WeaponItem = item;
                    break;
                case ItemType.Armor: //If it's an armor, it goes in the armor slot.
                    replacedItem = ArmorItem;
                    ArmorItem = item;
                    break;
                case ItemType.Relic: //If it's a relic, it goes in the relic slot.
                    RelicItems.Enqueue(item);
                    if (RelicItems.Count > 3) replacedItem = RelicItems.Dequeue();
                    break;
                case ItemType.Tower:
                    return null;
            }

            return replacedItem;
        }
    }
}