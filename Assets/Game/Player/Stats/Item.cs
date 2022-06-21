using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Weapon, Armor, Relic
}

public enum ItemRarity
{
    Common, Uncommon, Rare, Unique
}

public class Item
{
    public Sprite UISprite;
    public string ItemName;
    public ItemType ItemType;
    public ItemRarity ItemRarity;
    public StatBonus[] StatBonuses;
}
