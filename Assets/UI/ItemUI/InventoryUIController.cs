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
            if (PlayerInventory.Instance.WeaponItem != null)
            {
                weaponText.text = GetItemDescription(PlayerInventory.Instance.WeaponItem);
                weaponImage.sprite = PlayerInventory.Instance.WeaponItem.uiSprite;
            }

            if (PlayerInventory.Instance.ArmorItem != null)
            {
                armorText.text = GetItemDescription(PlayerInventory.Instance.ArmorItem);
                armorImage.sprite = PlayerInventory.Instance.ArmorItem.uiSprite;
            }

            if (PlayerInventory.Instance.RelicItem1 != null)
            {
                relic1Image.sprite = PlayerInventory.Instance.RelicItem1.uiSprite;
            }

            if (PlayerInventory.Instance.RelicItem2 != null)
            {
                relic2Image.sprite = PlayerInventory.Instance.RelicItem2.uiSprite;
            }

            if (PlayerInventory.Instance.RelicItem3 != null)
            {
                relic3Image.sprite = PlayerInventory.Instance.RelicItem3.uiSprite;
            }
            //

            moneyText.text = GameManager.Money.ToString();
        }

        private string GetItemDescription(Item item) //Returns a string that contains a specified item's name and stats, all on their own line
        {
            var stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine(item.itemName); //Append the item name
            
            if(item.IsEquippable) stringBuilder.AppendLine(PlayerStats.GetStatsText(item.statBonuses));

            return stringBuilder.ToString(); //Return the string
        }
    }
}
