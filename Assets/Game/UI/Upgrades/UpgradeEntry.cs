using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using TonyDev.Game.Core.Behavior;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public enum UpgradeCategory
    {
        Offensive, Defensive, Utility, Crystal
    }
    
    public class UpgradeEntry : MonoBehaviour
    {
        //References to UI elements
        
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text costText;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;

        [SerializeField] private Button purchaseButton;

        /**********************************/

        [NonSerialized] public UpgradeData OriginalData;
        
        private int _cost; // Cost of the upgrade in essence.

        public Func<bool> PurchasableFunc = () => false; // Should this item be purchasable? Influences if can click button and purchase.

        private int _id; // Unique id used to call our purchase function in UpgradeManager

        //private bool _local; // Is this upgrade a local upgrade only? If false, purchase code will be ran on server and only purchasable once. If true, purchasable on each client once.

        private bool _set; // Has this class had the 'set' method called? If not, shouldn't allow it to be purchased

        private UpgradeCategory _category; // Keep track of the category for checking filter visibility.
        
        public string UpgradeName => nameText.text; // Used to get a reference to the upgrade's name upon checking prereqs
        
        public void TryPurchase() // Called when clicking "Buy" button
        {
            if (GameManager.Money < _cost || !PurchasableFunc.Invoke() || !_set) return; // Check if purchasable.

            GameManager.Money -= _cost; // Remove the cost from player's essence balance

            // Different methods to notify purchase depending on if we are a local upgrade.
            UpgradeManager.Instance.NotifyPurchase(_id);

            PurchasableFunc = () => false; // Cannot purchase multiple times. TODO: May add functionality for repeat purchases in the future.
        }

        // Called in UpgradeManager
        public void Set(int scrapCost, Sprite icon, string upgradeName, string description, Func<bool> isPurchasable,
            int id, UpgradeCategory category, UpgradeData data)
        {
            if (_set) return; // Don't allow to be set twice. New object should be instantiated in that case.
            
            // Update our variables based on supplied values
            _cost = scrapCost;
            costText.text = _cost == 0 ? "FREE" : _cost.ToString();
            descriptionText.text = description;
            nameText.text = upgradeName;
            iconImage.sprite = icon;
            OriginalData = data;

            PurchasableFunc = isPurchasable;

            _category = category;
            
            backgroundImage.color = category switch
            {
                UpgradeCategory.Offensive => new Color(120/255f, 29/255f, 79/255f, 1f),
                UpgradeCategory.Defensive => new Color(59/255f, 125/255f, 79/255f, 1f),
                UpgradeCategory.Utility => new Color(207/255f, 117/255f, 43/255f, 1f),
                UpgradeCategory.Crystal => new Color(173/255f, 47/255f, 69/255f, 1f),
                _ => Color.white
            };

            _id = id;

            _set = true;
            
            CheckShouldBeActive().Forget();
            /*****************************************/
        }

        private void Update()
        {
            purchaseButton.interactable = GameManager.Money >= _cost; // Don't allow button to be clicked if can't afford or purchase
        }

        private async UniTask CheckShouldBeActive()
        {
            while (this != null)
            {
                gameObject.SetActive(PurchasableFunc.Invoke() && UpgradeManager.Instance.filter.Contains(_category));
                await UniTask.WaitForFixedUpdate();
            }
        }
    }
}