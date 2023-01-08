using System;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev.Game.UI.Inventory
{
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        //Editor variables
        [SerializeField] private GameObject inventoryObject;
        [SerializeField] private GameObject gearInventoryObject;
        [SerializeField] private GameObject towerInventoryObject;
        [SerializeField] private GameObject statInventoryObject;
        [SerializeField] private ItemSlot weaponSlot;
        [SerializeField] private ItemSlot armorSlot;
        [SerializeField] private ItemSlot[] relicSlots;
        [SerializeField] private TMP_Text essenceText;
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

        [SerializeField] private TMP_Text cooldownText;
        //

        private void Awake()
        {
            Player.OnLocalPlayerCreated += Init;
            Minimized = true;
        }

        private void Init()
        {
            Player.LocalInstance.Stats.OnStatsChanged += UpdateStatText;
            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(true);
            statInventoryObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Player.OnLocalPlayerCreated -= Init;
        }

        private void UpdateStatText()
        {
            damageText.text = PlayerStats.GetStatsText(new[] {Stat.Damage}, false, false);
            healthText.text = PlayerStats.GetStatsText(new[] {Stat.Health}, false, false);
            regenText.text = PlayerStats.GetStatsText(new[] {Stat.HpRegen}, false, false);
            aoeText.text = PlayerStats.GetStatsText(new[] {Stat.AoeSize}, false, false);
            armorText.text = PlayerStats.GetStatsText(new[] {Stat.Armor}, false, false);
            speedText.text = PlayerStats.GetStatsText(new[] {Stat.MoveSpeed}, false, false);
            critText.text = PlayerStats.GetStatsText(new[] {Stat.CritChance}, false, false);
            dodgeText.text = PlayerStats.GetStatsText(new[] {Stat.Dodge}, false, false);
            attackSpeedText.text = PlayerStats.GetStatsText(new[] {Stat.AttackSpeed}, false, false);
            cooldownText.text = PlayerStats.GetStatsText(new[] {Stat.CooldownReduce}, false, false);
        }

        private bool m_Minimized = true;
        private bool Minimized
        {
            get { return m_Minimized; }
            set
            {
                m_Minimized = value;
                if (animator != null)
                {
                    animator.SetBool("minimized", value);
                }
            }
        }

        private void Update()
        {
            //Update all the UI elements
            weaponSlot.Item = PlayerInventory.Instance.WeaponItem;
            armorSlot.Item = PlayerInventory.Instance.ArmorItem;

            var relicArray = PlayerInventory.Instance.RelicItems.ToArray();

            for (var i = 0; i < relicSlots.Length; i++)
            {
                relicSlots[i].gameObject.SetActive(i + 1 <= PlayerInventory.Instance.RelicSlotCount);
                if (relicArray.Length >= i + 1) relicSlots[i].Item = relicArray[i];
            }
            //

            essenceText.text = GameManager.Essence.ToString();
            moneyText.text = GameManager.Money.ToString();
        }

        private float _lastToggleTime;

        public void OnToggleInventory(InputValue value)
        {
            if (!GameManager.GameControlsActive) return;

            if (!value.isPressed) return;

            Minimized = !Minimized;
        }

        public void SwitchToGearPanel()
        {
            SoundManager.PlaySound("button", 0.5f, Player.LocalInstance.transform.position);

            if (gearInventoryObject.activeSelf && !Minimized)
            {
                Minimized = true;
                return;
            }

            if (Minimized)
            {
                Minimized = false;
            }

            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(true);
            statInventoryObject.SetActive(false);
        }

        public void SwitchToTowerPanel()
        {
            SoundManager.PlaySound("button", 0.5f, Player.LocalInstance.transform.position);

            if (towerInventoryObject.activeSelf && !Minimized)
            {
                Minimized = true;
                return;
            }

            if (Minimized)
            {
                Minimized = false;
            }

            towerInventoryObject.SetActive(true);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(false);
        }

        public void SwitchToStatPanel()
        {
            SoundManager.PlaySound("button", 0.5f, Player.LocalInstance.transform.position);

            if (statInventoryObject.activeSelf && !Minimized)
            {
                Minimized = true;
                return;
            }

            if (Minimized)
            {
                Minimized = false;
            }

            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(true);
        }
    }
}