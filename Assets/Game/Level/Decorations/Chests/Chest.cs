using TonyDev.Game.Core.Items;
using TonyDev.Game.Level.Rooms;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Decorations.Chests
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private Animator chestAnimator;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.isTrigger)
            {
                DropItems(); //Drop items when player touches the chest
            }
        }

        private void DropItems()
        {
            //Roll for a random rarity
            var rarityRoll = Random.Range(0, 100);
            var itemRarity = rarityRoll switch
            {
                var n when n >= 95 => ItemRarity.Unique, //5% chance for unique
                var n when n >= 75 => ItemRarity.Rare, //20% chance for rare
                var n when n >= 40 => ItemRarity.Uncommon, //35% chance for uncommon
                var n when n >= 0 => ItemRarity.Common, //40% chance for common
                _ => ItemRarity.Common
            };
            //

            var item = ItemGenerator.GenerateItem((ItemType) Random.Range(0, 2), itemRarity); //Generate an item to drop
        
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
}
