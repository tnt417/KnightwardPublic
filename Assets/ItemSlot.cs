using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game.Core.Items;
using TonyDev.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class ItemSlot : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public Image image;
        [SerializeField] public TMP_Text nameText;
        [SerializeField] public TMP_Text descriptionText;
        //
        
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

        private void OnItemSet(Item item)
        {
            if (image != null) image.sprite = Item.uiSprite;
            if (nameText != null) nameText.text = item.itemName;
            if(descriptionText != null) descriptionText.text = item.GetItemDescription();
        }
        
        public void OnClick()
        {
            if(Item.itemType is ItemType.Tower) TowerUIController.Instance.StartPlacingTower(this, Item);
        }
    }
}
