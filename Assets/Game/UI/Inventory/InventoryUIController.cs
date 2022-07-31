using System.Globalization;
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
        [SerializeField] private GameObject statInventoryObject;
        [SerializeField] private ItemSlot weaponSlot;
        [SerializeField] private ItemSlot armorSlot;
        [SerializeField] private ItemSlot[] relicSlots;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text regenText;
        [SerializeField] private TMP_Text aoeText;
        [SerializeField] private TMP_Text armorText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text critText;
        [SerializeField] private TMP_Text dodgeText;
        [SerializeField] private TMP_Text attackSpeedText;
        //

        private void Awake()
        {
            PlayerStats.OnStatsChanged += UpdateStatText;
        }

        private void UpdateStatText()
        {
            damageText.text = PlayerStats.GetStatsText(new []{Stat.Damage}, false, false);
            healthText.text = PlayerStats.GetStatsText(new []{Stat.Health}, false, false);
            regenText.text = PlayerStats.GetStatsText(new []{Stat.HpRegen}, false, false);
            aoeText.text = PlayerStats.GetStatsText(new []{Stat.AoeSize}, false, false);
            armorText.text = PlayerStats.GetStatsText(new []{Stat.Armor}, false, false);
            speedText.text = PlayerStats.GetStatsText(new []{Stat.MoveSpeed}, false, false);
            critText.text = PlayerStats.GetStatsText(new []{Stat.CritChance}, false, false);
            dodgeText.text = PlayerStats.GetStatsText(new []{Stat.Dodge}, false, false);
            attackSpeedText.text = PlayerStats.GetStatsText(new []{Stat.AttackSpeed}, false, false);
        }
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)  && GameManager.GameControlsActive) //Toggle the inventory panel when tab is pressed.
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
            statInventoryObject.SetActive(false);
        }

        public void SwitchToTowerPanel()
        {
            towerInventoryObject.SetActive(true);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(false);
        }

        public void SwitchToStatPanel()
        {
            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(true);
        }
    }
}
