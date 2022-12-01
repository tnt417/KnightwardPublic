using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.UI.Tower;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TonyDev.Game.Core.Items
{
    public class PlayerInventory
    {
        static PlayerInventory()
        {
            Instance = new PlayerInventory();
        }

        public void Clear()
        {
            WeaponItem = null;
            ArmorItem = null;
            RelicItems.Clear();
        }

        public static readonly PlayerInventory Instance;

        //5 item variables, 1 for each inventory slot. A bit inefficient but works for now.
        public Item WeaponItem { get; private set; }
        public Item ArmorItem { get; private set; }

        public Queue<Item> RelicItems { get; private set; } = new();

        public static Action<Item> OnItemInsertLocal;

        public void InsertStarterItems()
        {
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Weapon,
                ItemRarity.Common)); //Add a starting sword to the player's inventory.
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Armor,
                ItemRarity.Common)); //Add armor to inventory
            InsertItem(ItemGenerator.GenerateItemOfType(ItemType.Tower,
                ItemRarity.Common)); //Add armor to inventory
        }

        private static void InsertTower(Item item) //Inserts a tower into the inventory and adds it to the UI
        {
            if (!item.IsSpawnable) return;
            TowerUIController.Instance.AddTower(item);
        }

        public Item GetSwap(Item item)
        {
            Item replacement = null; //Holds the replaced item to be returned at the end.
            switch (item.itemType)
            {
                case ItemType.Weapon: //If it's a weapon, it goes in the weapon slot.
                    replacement = WeaponItem;
                    break;
                case ItemType.Armor: //If it's an armor, it goes in the armor slot.
                    replacement = ArmorItem;
                    break;
                case ItemType.Relic: //If it's a relic, it goes in the relic slot.
                    replacement = RelicItems.FirstOrDefault(i => i.itemName == item.itemName);
                    break;
                case ItemType.Tower:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return replacement == item ? null : replacement;
        }
        
        //Replaces/inserts items into inventory and returns the item that was replaced, if any.
        public Item InsertItem(Item item)
        {
            if (item.itemType == ItemType.Tower)
            {
                InsertTower(item); //If it's a tower, insert it
                OnItemInsertLocal?.Invoke(item);
                return null;
            }

            var replacedItem = SwapSlot(item); //Holds the replaced item to be returned at the end.

            if (replacedItem == item) return item; //If swapping slot was "rejected", don't continue on.

            if (item.IsEquippable)
            {
                if (replacedItem != null)
                {
                    if (replacedItem.itemEffects != null)
                    {
                        foreach (var effect in replacedItem.itemEffects) Player.LocalInstance.CmdRemoveEffect(effect);
                    }

                    PlayerStats.Stats.RemoveStatBonuses(replacedItem
                        .itemName); //Remove stat bonuses of the now removed item
                }

                if (item.itemEffects != null)
                    foreach (var effect in item.itemEffects)
                    {
                        Player.LocalInstance.CmdAddEffect(effect, Player.LocalInstance);
                    }

                PlayerStats.AddStatBonusesFromItem(item); //Apply stat bonuses of the new item
            }

            OnItemInsertLocal?.Invoke(item);

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
                    if (RelicItems.Any(i => i.itemName == item.itemName))
                    {
                        while (true)
                        {
                            replacedItem = RelicItems.Dequeue();

                            if (replacedItem.itemName == item.itemName)
                            {
                                break;
                            }
                            RelicItems.Enqueue(replacedItem);
                        }
                    }

                    RelicItems.Enqueue(item);
                    if (RelicItems.Count > 3) replacedItem = RelicItems.Dequeue();
                    break;
                case ItemType.Tower:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return replacedItem;
        }
    }
}