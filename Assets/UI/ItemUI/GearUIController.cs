using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearUIController : MonoBehaviour
{
    //Editor variables
    [SerializeField] private GameObject gearPanelObject;
    [SerializeField] private Image weaponImage;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private Image armorImage;
    [SerializeField] private Image relic1Image;
    [SerializeField] private Image relic2Image;
    [SerializeField] private Image relic3Image;
    //
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) //Toggle the inventory panel when I is pressed.
        {
            gearPanelObject.SetActive(!gearPanelObject.activeSelf);
        }
        
        //Update all the UI elements
        if (PlayerInventory.Instance.WeaponItem != null)
        {
            weaponText.text = GetItemDescription(PlayerInventory.Instance.WeaponItem); //TODO: weapon text
            weaponImage.sprite = PlayerInventory.Instance.WeaponItem.UISprite;
        }

        if (PlayerInventory.Instance.ArmorItem != null)
        {
            armorImage.sprite = PlayerInventory.Instance.ArmorItem.UISprite;
        }

        if (PlayerInventory.Instance.RelicItem1 != null)
        {
            relic1Image.sprite = PlayerInventory.Instance.RelicItem1.UISprite;
        }

        if (PlayerInventory.Instance.RelicItem2 != null)
        {
            relic2Image.sprite = PlayerInventory.Instance.RelicItem2.UISprite;
        }

        if (PlayerInventory.Instance.RelicItem3 != null)
        {
            relic3Image.sprite = PlayerInventory.Instance.RelicItem3.UISprite;
        }
        //
    }

    private string GetItemDescription(Item item) //Returns a string that contains a specified item's name and stats, all on their own line
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(PlayerInventory.Instance.WeaponItem.ItemName + "\n"); //Append the item name
        foreach (var sb in PlayerInventory.Instance.WeaponItem.StatBonuses) //Append the stat bonuses
        {
            stringBuilder.Append(sb.stat + ": " + sb.strength + "\n");
        }

        return stringBuilder.ToString(); //Return the string
    }
}
