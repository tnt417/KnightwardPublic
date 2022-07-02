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
        [SerializeField] private GameObject gearInventoryObject;
        [SerializeField] private GameObject towerInventoryObject;
        [SerializeField] private ItemSlot weaponSlot;
        [SerializeField] private ItemSlot armorSlot;
        [SerializeField] private ItemSlot[] relicSlots;
        [SerializeField] private TMP_Text moneyText;
        //
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) //Toggle the inventory panel when tab is pressed.
            {
                inventoryObject.SetActive(!inventoryObject.activeSelf);
            }
        
            //Update all the UI elements
            weaponSlot.Item = PlayerInventory.Instance.WeaponItem;
            armorSlot.Item = PlayerInventory.Instance.ArmorItem;

            var relicArray = PlayerInventory.Instance.RelicItems.ToArray();
            if(relicArray.Length >= 1) relicSlots[0].Item = relicArray[0];
            if(relicArray.Length >= 2) relicSlots[1].Item = relicArray[1];
            if(relicArray.Length >= 3) relicSlots[2].Item = relicArray[2];
            //

            moneyText.text = GameManager.Money.ToString();
        }

        public void SwitchToGearPanel()
        {
            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(true);
        }

        public void SwitchToTowerPanel()
        {
            towerInventoryObject.SetActive(true);
            gearInventoryObject.SetActive(false);
        }
    }
}
