using System.Collections;
using TMPro;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Chests;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace TonyDev.Game.Core.Items
{
    public class GroundItem : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private int rarityBoost;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject pickupIndicator;
        [SerializeField] private GameObject moneyIcon;
        [SerializeField] private TMP_Text moneyLabel;
        [SerializeField] private int cost;
        //
        
        private Item _item;
        private bool _pickupAble = true;
        private bool FromChest => GetComponentInParent<Chest>() != null;

        public UnityEvent onPickup = new UnityEvent();

        private void Awake()
        {
            spriteRenderer.sharedMaterial = new Material(spriteRenderer.sharedMaterial); //Create a copy of the renderer's material to allow temporary editing.
        }

        private void Start()
        {
            if (!FromChest) SetItem(ItemGenerator.GenerateItem(rarityBoost));
            SetCost(FromChest || cost == 0 ? 0 : ItemGenerator.GenerateCost(_item));
        }
    
        //Set the GroundItem's item.
        private void SetItem(Item newItem)
        {
            if (newItem == null)
            {
                Destroy(gameObject);
                return;
            }
            
            spriteRenderer.sprite = newItem.uiSprite; //Update the sprite
            _item = newItem; //Update the item
            UpdateOutlineColor(); //Update the outline color
        }

        private void SetCost(int newCost)
        {
            cost = newCost;
            moneyIcon.SetActive(cost != 0);
            moneyLabel.enabled = cost != 0;
            moneyLabel.text = cost.ToString();
        }

        private void UpdateOutlineColor()
        {
            switch (_item.itemRarity) //Set the material's outline color based on the rarity. These are hardcoded right now.
            {
                case ItemRarity.Common:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.black);
                    break;
                case ItemRarity.Uncommon:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.green);
                    break;
                case ItemRarity.Rare:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.yellow);
                    break;
                case ItemRarity.Unique:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.red);
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) pickupIndicator.SetActive(true); //Show pickup indicator when the player is on top of the item
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (Input.GetKey(KeyCode.E) && _pickupAble && GameManager.Money >= cost) //When the player presses E and has sufficient money...
            {
                var returnItem = PlayerInventory.Instance.InsertItem(_item); //...try to insert the item into the player's inventory
                GameManager.Money -= cost; //Subtract money
                onPickup.Invoke(); //Invoke the onPickup method
                if(returnItem == null) Destroy(gameObject); //If no item was replaced, just destroy this GroundItem
                else{
                    SetItem(returnItem); //Otherwise, replaced the item
                    SetCost(0); //Don't make the player pay for their replaced item
                }
                StartCoroutine(DisablePickupForSeconds(0.5f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item
            }
        }

        private IEnumerator DisablePickupForSeconds(float seconds) //Disables pickup for a specified number of seconds
        {
            _pickupAble = false;
            yield return new WaitForSeconds(seconds);
            _pickupAble = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) pickupIndicator.SetActive(false); //Deactivate pickup indicator when the player is no longer on top of the item
        }
    }
}
