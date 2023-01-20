using System;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.UI.Tower;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Inventory
{
    public class ItemSlot : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public Image image;
        [SerializeField] public TMP_Text nameText;
        [SerializeField] public TMP_Text descriptionText;
        //
        
        private int _insertionFloor;
        
        private Item _itemValue;
        public Item Item
        {
            get => _itemValue;
            set
            { 
                _itemValue = value;   
                OnItemSet(value);
            }
        }

        private void Update()
        {
            image.enabled = image.sprite != null;
        }

        private void Awake()
        {
            _insertionFloor = GameManager.DungeonFloor;
        }

        private void OnItemSet(Item item)
        {
            if (image != null)
            {
                image.sprite = item?.uiSprite;
            }
            else
            {
                image.enabled = false;
            }
            if (nameText != null) nameText.text = item?.itemName;
            if(descriptionText != null) descriptionText.text = item?.GetItemDescription();
        }
        
        public void OnClick()
        {
            SoundManager.PlaySound("button",0.5f, Player.LocalInstance.transform.position);
            if(Item.itemType is ItemType.Tower) TowerUIController.Instance.StartPlacingTower(this, Item);
        }

        public void DestroyItem()
        {
            var essence = ItemGenerator.GenerateEssence(Item, _insertionFloor);

            SoundManager.PlaySound("interact",0.5f, Player.LocalInstance.transform.position, 0.8f);
            
            GameManager.Essence += essence;
            
            ObjectSpawner.SpawnTextPopup(Player.LocalInstance.transform.position, "+" + essence + " essence", Color.cyan, 0.8f);
            
            Destroy(gameObject);
        }

        public void DropItem()
        {
            GameManager.Instance.CmdDropItem(Item, Player.LocalInstance);
            
            Destroy(gameObject);
        }
    }
}
