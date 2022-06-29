using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.UI.ItemUI
{
    public class InventoryUIController : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private GameObject inventoryObject;
        [SerializeField] private Image weaponImage;
        [SerializeField] private TMP_Text weaponText;
        [SerializeField] private Image armorImage;
        [SerializeField] private TMP_Text armorText;
        [SerializeField] private Image relic1Image;
        [SerializeField] private Image relic2Image;
        [SerializeField] private Image relic3Image;
        [SerializeField] private TMP_Text moneyText;
        //
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) //Toggle the inventory panel when tab is pressed.
            {
                inventoryObject.SetActive(!inventoryObject.activeSelf);
            }
        
            //Update all the UI elements
            var weaponItem = PlayerInventory.Instance.WeaponItem;
            if (weaponItem != null)
            {
                weaponText.text = weaponItem.itemDescription + "\n" + GetItemDescription(weaponItem);
                weaponImage.sprite = weaponItem.uiSprite;
            }

            var armorItem = PlayerInventory.Instance.ArmorItem;
            if (armorItem != null)
            {
                armorText.text = armorItem.itemDescription + "\n" + GetItemDescription(armorItem);
                armorImage.sprite = armorItem.uiSprite;
            }

            if (PlayerInventory.Instance.RelicItems.Count >= 3)
            {
                relic3Image.enabled = true;
                relic3Image.sprite = PlayerInventory.Instance.RelicItems.ToArray()[2].uiSprite;
            }
            else relic3Image.enabled = false;

            if (PlayerInventory.Instance.RelicItems.Count >= 2)
            {
                relic2Image.enabled = true;
                relic2Image.sprite = PlayerInventory.Instance.RelicItems.ToArray()[1].uiSprite;
            }
            else relic2Image.enabled = false;
            
            if (PlayerInventory.Instance.RelicItems.Count >= 1)
            {
                relic1Image.enabled = true;
                relic1Image.sprite = PlayerInventory.Instance.RelicItems.ToArray()[0].uiSprite;
            }
            else relic1Image.enabled = false;
            //

            moneyText.text = GameManager.Money.ToString();
        }

        private string GetItemDescription(Item item) //Returns a string that contains a specified item's name and stats, all on their own line
        {
            var stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine(item.itemName); //Append the item name
            stringBuilder.AppendLine(item.itemDescription + "\n");

            if(item.IsEquippable) stringBuilder.AppendLine("<color=grey>" + PlayerStats.GetStatsText(item.statBonuses) + "</color>" + "\n");

            return stringBuilder.ToString(); //Return the string
        }
    }
}
