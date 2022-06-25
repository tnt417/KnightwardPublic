using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    
    //5 item variables, 1 for each inventory slot. A bit inefficient but works for now.
    public Item WeaponItem { get; private set; }
    public Item ArmorItem{ get; private set; }
    public Item RelicItem1{ get; private set; }
    public Item RelicItem2{ get; private set; }
    public Item RelicItem3{ get; private set; }
    //
    
    private void Start()
    {
        InsertItem(ItemGenerator.GenerateItem(ItemType.Weapon, ItemRarity.Common)); //Add a starting sword to the player's inventory.
        InsertItem(ItemGenerator.GenerateItem(ItemType.Armor, ItemRarity.Common)); //Add armor to inventory
    }
    
    private void Awake()
    {
        //Singleton code
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
        //
    }

    
    //Replaces/inserts items into inventory and returns the item that was replaced, if any.
    public Item InsertItem(Item item)
    {
        Item replacedItem = null; //Holds the replaced item to be returned at the end.
        switch (item.ItemType)
        {
            case ItemType.Weapon: //If it's a weapon, it goes in the weapon slot.
                replacedItem = WeaponItem;
                WeaponItem = item;
                break;
            case ItemType.Armor: //If it's an armor, it goes in the armor slot.
                replacedItem = ArmorItem;
                ArmorItem = item;
                break;
            case ItemType.Relic: //If it's a relic, it goes in the relic slot. //TODO: figure out relic replacing
                if(RelicItem1 == null) RelicItem1 = item;
                else if(RelicItem2 == null) RelicItem2 = item;
                else if(RelicItem3 == null) RelicItem3 = item;
                break;
            default:
                break;
        }

        if (replacedItem != null) PlayerStats.RemoveStatBonuses(replacedItem.ItemName); //Remove stat bonuses of the now removed item
        PlayerStats.AddStatBonusesFromItem(item); //Apply stat bonuses of the new item

        return replacedItem; //Return the item that was replaced. If nothing was replaced, returns null.
    }
}
