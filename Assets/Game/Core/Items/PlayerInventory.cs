using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities;
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
            RelicItems.Clear();
        }

        public static readonly PlayerInventory Instance;

        //5 item variables, 1 for each inventory slot. A bit inefficient but works for now.
        public Item WeaponItem { get; private set; }

        public Queue<Item> RelicItems { get; private set; } = new();

        public int RelicSlotCount { get; private set; } = 100;

        public void AddRelicSlot()
        {
            //RelicSlotCount++;
        }

        public static Action<Item> OnItemInsertLocal;

        public static Action<Item> OnItemRemoveLocal;
        
        public void InsertStarterItems()
        {
            var swordItem = Object.Instantiate(GameManager.AllItems.FirstOrDefault(i => i.item.itemName == "Broadsword")).item;
            InsertItem(new Item()
            {
                itemName = "Starter Sword",
                bypassStatGeneration = false,
                itemDescription = "A weak starter sword.",
                itemEffects = swordItem.itemEffects,
                itemRarity = ItemRarity.Common,
                itemType = ItemType.Weapon, 
                projectiles = swordItem.projectiles,
                spawnablePrefabName = "",
                statBonuses = new StatBonus[]
                {
                    new StatBonus(StatType.Flat, Stat.Damage, 20, "Starter Sword"),
                    new StatBonus(StatType.Flat, Stat.AttackSpeed, 1.5f, "Starter Sword")
                },
                uiSprite = swordItem.uiSprite
            });

            var towerItem = Object.Instantiate(GameManager.AllItems.FirstOrDefault(i => i.item.itemName == "Ballista Tower")).item;

            InsertItem(new Item()
            {
                itemName = "Starter Tower",
                bypassStatGeneration = true,
                itemDescription = "A tower used to defend your crystal.",
                itemEffects = towerItem.itemEffects,
                itemRarity = ItemRarity.Common,
                itemType = ItemType.Tower, 
                projectiles = towerItem.projectiles,
                spawnablePrefabName = "ballista",
                statBonuses = new []
                {
                    new StatBonus(StatType.Flat, Stat.Damage, 35, "Starter Tower"),
                    new StatBonus(StatType.Flat, Stat.AttackSpeed, 1f, "Starter Tower"),
                    new StatBonus(StatType.Flat, Stat.Health, 1000000f, "Starter Tower", true)
                },
                uiSprite = towerItem.uiSprite
            });
            
            var regenItem = Object.Instantiate(GameManager.AllItems.FirstOrDefault(i => i.item.itemName == "Regeneration Tower")).item;

            InsertItem(new Item()
            {
                itemName = "Starter Regenerator",
                bypassStatGeneration = true,
                itemDescription = "A tower used to repair your crystal.",
                itemEffects = regenItem.itemEffects,
                itemRarity = ItemRarity.Common,
                itemType = ItemType.Tower, 
                projectiles = regenItem.projectiles,
                spawnablePrefabName = "regen",
                statBonuses = new []
                {
                    new StatBonus(StatType.Flat, Stat.Damage, 10, "Starter Regen"),
                    new StatBonus(StatType.Flat, Stat.AttackSpeed, 0.5f, "Starter Regen"),
                    new StatBonus(StatType.Flat, Stat.Health, 1000000f, "Starter Regen", true)
                },
                uiSprite = regenItem.uiSprite
            });
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
                OnUnEquip(replacedItem);

                if (item.itemEffects != null)
                    foreach (var effect in item.itemEffects)
                    {
                        Player.LocalInstance.AddEffect(effect, Player.LocalInstance);
                    }

                PlayerStats.AddStatBonusesFromItem(item); //Apply stat bonuses of the new item
            }

            OnItemInsertLocal?.Invoke(item);

            return replacedItem; //Return the item that was replaced. If nothing was replaced, returns null.
        }

        public void OnUnEquip(Item item)
        {
            if (item != null)
            {
                if (item.itemEffects != null)
                {
                    foreach (var effect in item.itemEffects) Player.LocalInstance.RemoveEffect(effect);
                }

                PlayerStats.Stats.RemoveStatBonuses(item
                    .itemName); //Remove stat bonuses of the now removed item
                
                OnItemRemoveLocal?.Invoke(item);
            }
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
                    if (RelicItems.Count > RelicSlotCount) replacedItem = RelicItems.Dequeue();
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