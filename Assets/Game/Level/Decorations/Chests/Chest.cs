using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Decorations.Chests
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private int rarityBoost;
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private ItemData[] possibleTowerItems;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.isTrigger)
            {
                DropItems(); //Drop items when player touches the chest
            }
        }

        private void DropItems()
        {

            var itemType = Item.RandomItemType;

            var item = itemType switch
            {
                ItemType.Weapon or ItemType.Armor or ItemType.Relic => ItemGenerator.GenerateEquippableItem(
                    Item.RandomItemType, Item.RandomRarity(rarityBoost)),
                ItemType.Tower => possibleTowerItems[Random.Range(0, possibleTowerItems.Length)].item,
                _ => null
            };

            Debug.Log("Item type: " + item?.itemType + ", Item sprite: " + item?.uiSprite);

            //If in a room, make the room the parent of the new items
            Transform parentTransform = null;
            var parentRoom = GetComponentInParent<Room>();
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
