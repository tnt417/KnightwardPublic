using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.isTrigger)
            {
                DropItems(); //Drop items when player touches the chest
            }
        }

        private void DropItems()
        {
            var item = ItemGenerator.GenerateItem(rarityBoost);

            GameConsole.Log("Item type: " + item?.itemType + ", Item sprite: " + item?.uiSprite);
            
            var parentTransform = transform;

            //Instantiate the item and change the chest sprite
            var groundItem = Instantiate(groundItemPrefab, parentTransform.position, quaternion.identity, parentTransform);
            groundItem.SendMessage("SetItem", item);
            chestAnimator.Play("ChestOpen");
            //
        
            Destroy(this); //Destroy this script so no more items drop.
        }
    }
}
