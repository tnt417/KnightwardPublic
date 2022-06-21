using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private GameObject groundItemPrefab;
    [SerializeField] private ItemRarity itemRarity;
    [SerializeField] private Animator chestAnimator;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DropItems(); //Drop items when player touches the chest
        }
    }

    private void DropItems()
    {
        var item = ItemGenerator.GenerateItem(ItemType.Weapon, itemRarity); //Generate an item to drop
        
        //If in a room, make the room the parent of the new items
        Transform parentTransform = null;
        Room parentRoom = GetComponentInParent<Room>();
        if (parentRoom != null) parentTransform = parentRoom.transform;
        //
        
        //Instantiate the item and change the chest sprite
        var groundItem = Instantiate(groundItemPrefab, transform.position, quaternion.identity, parentTransform);
        groundItem.SendMessage("SetItem", item);
        chestAnimator.Play("ChestOpen");
        //
        
        Destroy(this); //Destroy this script so no more items drop.
    }
}
