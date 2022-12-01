using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class UpgradeEntry : MonoBehaviour
    {
        //References to UI elements
        
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text costText;

        [SerializeField] private Image iconImage;

        [SerializeField] private Button purchaseButton;

        /**********************************/        
        
        private int _cost; // Cost of the upgrade in essence.

        private Func<bool> _purchasableFunc = () => false; // Should this item be purchasable? Influences if can click button and purchase.

        private int _id; // Unique id used to call our purchase function in UpgradeManager

        private bool _local; // Is this upgrade a local upgrade only? If false, purchase code will be ran on server and only purchasable once. If true, purchasable on each client once.

        private bool _set; // Has this class had the 'set' method called? If not, shouldn't allow it to be purchased

        public void TryPurchase() // Called when clicking "Buy" button
        {
            if (GameManager.Essence < _cost || !_purchasableFunc.Invoke() || !_set) return; // Check if purchasable.

            GameManager.Essence -= _cost; // Remove the cost from player's essence balance

            // Different methods to notify purchase depending on if we are a local upgrade.
            if (_local) UpgradeManager.Instance.NotifyPurchaseLocal(_id);
            else UpgradeManager.Instance.CmdNotifyPurchaseGlobal(_id, Player.LocalInstance);

            _purchasableFunc = () => false; // Cannot purchase multiple times. TODO: May add functionality for repeat purchases in the future.
        }

        // Called in UpgradeManager
        public void Set(int scrapCost, Sprite icon, string upgradeName, string description, Func<bool> isPurchasable,
            int id, bool local)
        {
            if (_set) return; // Don't allow to be set twice. New object should be instantiated in that case.
            
            // Update our variables based on supplied values
            _cost = scrapCost;
            costText.text = _cost.ToString();
            descriptionText.text = description;
            nameText.text = upgradeName;
            iconImage.sprite = icon;

            _purchasableFunc = isPurchasable;

            _id = id;

            _local = local;

            _set = true;
            /*****************************************/
        }

        private void Update()
        {
            purchaseButton.interactable = GameManager.Essence >= _cost && _purchasableFunc.Invoke(); // Don't allow button to be clicked if can't afford or purchase
        }
    }
}