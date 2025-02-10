using System;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.UI.Tower;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        [SerializeField] private GameObject relicSlotPrefab;
        [SerializeField] private ItemSlot weaponSlot;
        [SerializeField] private ItemSlot armorSlot;
        [SerializeField] private GameObject relicLayout;
        private readonly Dictionary<Item, ItemSlot> _relicSlots = new();
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
        [SerializeField] private TMP_Text noTowersText;

        [SerializeField] private TMP_Text cooldownText;

        [SerializeField] private Button statsButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button towerButton;
        [SerializeField] private Sprite[] buttonSprites;
        //

        public static string ActivePanel = "";

        private void Awake()
        {
            Player.OnLocalPlayerCreated += Init;
            Minimized = true;
            ActivePanel = "None";
        }

        private void Init()
        {
            Player.LocalInstance.Stats.OnStatChanged += UpdateStatText;
            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(true);
            statInventoryObject.SetActive(false);

            PlayerInventory.OnItemInsertLocal += OnInsertItem;
            PlayerInventory.OnItemRemoveLocal += OnRemoveItem;
        }

        private void OnInsertItem(Item item)
        {
            if (item.itemType != ItemType.Relic) return;
            var slot = Instantiate(relicSlotPrefab, relicLayout.transform);
            var itemSlot = slot.GetComponent<ItemSlot>();
            itemSlot.Item = item;
                
            _relicSlots.Add(item, itemSlot);
        }

        private void OnRemoveItem(Item item)
        {
            if (item.itemType != ItemType.Relic) return;
            Destroy(_relicSlots[item].gameObject);
            _relicSlots.Remove(item);
        }

        private void OnDestroy()
        {
            PlayerInventory.OnItemInsertLocal -= OnInsertItem;
            PlayerInventory.OnItemRemoveLocal -= OnRemoveItem;
            Player.OnLocalPlayerCreated -= Init;
        }

        private void UpdateStatText(Stat statToBeUpdated)
        {
            var statText = statToBeUpdated switch
            {
                Stat.Damage => damageText,
                Stat.Health => healthText,
                Stat.HpRegen => regenText,
                Stat.AoeSize => aoeText,
                Stat.Armor => armorText,
                Stat.MoveSpeed => speedText,
                Stat.CritChance => critText,
                Stat.Dodge => dodgeText,
                Stat.AttackSpeed => attackSpeedText,
                Stat.CooldownReduce => cooldownText,
                _ => throw new ArgumentOutOfRangeException(nameof(statToBeUpdated), statToBeUpdated, null)
            };

            statText.text = PlayerStats.GetStatsText(new[] { statToBeUpdated }, false, false);
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

                    if (value)
                    {
                        var statsRectTransform = statsButton.transform.GetChild(0).GetComponent<RectTransform>();
                        statsRectTransform.anchoredPosition = new Vector2(statsRectTransform.anchoredPosition.x, 8f);
                        var towerRectTransform = towerButton.transform.GetChild(0).GetComponent<RectTransform>();
                        towerRectTransform.anchoredPosition = new Vector2(towerRectTransform.anchoredPosition.x, 8f);
                        var gearRectTransform = gearButton.transform.GetChild(0).GetComponent<RectTransform>();
                        gearRectTransform.anchoredPosition = new Vector2(gearRectTransform.anchoredPosition.x, 8f);
                        
                        statsButton.GetComponent<Image>().sprite = buttonSprites[0];
                        towerButton.GetComponent<Image>().sprite = buttonSprites[0];
                        gearButton.GetComponent<Image>().sprite = buttonSprites[0];
                    }
                }
            }
        }

        private void Update()
        {
            //Update all the UI elements
            weaponSlot.Item = PlayerInventory.Instance.WeaponItem;

            noTowersText.gameObject.SetActive(TowerUIController.Instance.Towers.Count == 0);
            
            // var relicArray = PlayerInventory.Instance.RelicItems.ToArray();
            //
            // for (var i = 0; i < _relicSlots.Length; i++)
            // {
            //     _relicSlots[i].gameObject.SetActive(i + 1 <= PlayerInventory.Instance.RelicSlotCount);
            //     if (relicArray.Length >= i + 1)
            //     {
            //         _relicSlots[i].Item = relicArray[i];
            //     }
            //     else
            //     {
            //         _relicSlots[i].Item = null;
            //     }
            // }
            //

            //essenceText.text = GameManager.Essence.ToString();
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
            
            statsButton.GetComponent<Image>().sprite = buttonSprites[0];
            towerButton.GetComponent<Image>().sprite = buttonSprites[0];
            gearButton.GetComponent<Image>().sprite = buttonSprites[1];

            var statsRectTransform = statsButton.transform.GetChild(0).GetComponent<RectTransform>();
            statsRectTransform.anchoredPosition = new Vector2(statsRectTransform.anchoredPosition.x, 8f);
            var towerRectTransform = towerButton.transform.GetChild(0).GetComponent<RectTransform>();
            towerRectTransform.anchoredPosition = new Vector2(towerRectTransform.anchoredPosition.x, 8f);
            var gearRectTransform = gearButton.transform.GetChild(0).GetComponent<RectTransform>();
            gearRectTransform.anchoredPosition = new Vector2(gearRectTransform.anchoredPosition.x, -8f);
            
            if (gearInventoryObject.activeSelf && !Minimized)
            {
                Minimized = true;
                return;
            }

            if (Minimized)
            {
                Minimized = false;
            }
            
            ActivePanel = "Gear";

            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(true);
            statInventoryObject.SetActive(false);
        }

        public void SwitchToTowerPanel()
        {
            statsButton.GetComponent<Image>().sprite = buttonSprites[0];
            towerButton.GetComponent<Image>().sprite = buttonSprites[1];
            gearButton.GetComponent<Image>().sprite = buttonSprites[0];
            
            var statsRectTransform = statsButton.transform.GetChild(0).GetComponent<RectTransform>();
            statsRectTransform.anchoredPosition = new Vector2(statsRectTransform.anchoredPosition.x, 8f);
            var towerRectTransform = towerButton.transform.GetChild(0).GetComponent<RectTransform>();
            towerRectTransform.anchoredPosition = new Vector2(towerRectTransform.anchoredPosition.x, -8f);
            var gearRectTransform = gearButton.transform.GetChild(0).GetComponent<RectTransform>();
            gearRectTransform.anchoredPosition = new Vector2(gearRectTransform.anchoredPosition.x, 8f);
            
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
            
            ActivePanel = "Tower";

            towerInventoryObject.SetActive(true);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(false);
        }

        public void SwitchToStatPanel()
        {
            statsButton.GetComponent<Image>().sprite = buttonSprites[1];
            towerButton.GetComponent<Image>().sprite = buttonSprites[0];
            gearButton.GetComponent<Image>().sprite = buttonSprites[0];
            
            var statsRectTransform = statsButton.transform.GetChild(0).GetComponent<RectTransform>();
            statsRectTransform.anchoredPosition = new Vector2(statsRectTransform.anchoredPosition.x, -8f);
            var towerRectTransform = towerButton.transform.GetChild(0).GetComponent<RectTransform>();
            towerRectTransform.anchoredPosition = new Vector2(towerRectTransform.anchoredPosition.x, 8f);
            var gearRectTransform = gearButton.transform.GetChild(0).GetComponent<RectTransform>();
            gearRectTransform.anchoredPosition = new Vector2(gearRectTransform.anchoredPosition.x, 8f);
            
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
            
            ActivePanel = "Stat";

            towerInventoryObject.SetActive(false);
            gearInventoryObject.SetActive(false);
            statInventoryObject.SetActive(true);
        }
    }
}