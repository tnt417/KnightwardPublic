using System;
using System.Collections.Generic;
using TonyDev.Game.Core.Items;
using UnityEngine;

namespace TonyDev.Game.UI.Unlocks
{
    public class UnlockUIController : MonoBehaviour
    {
        
        [SerializeField] private UpgradeChoiceController[] upgradeChoices;
        
        private Dictionary<UpgradeChoiceController, ItemData> _itemDatas = new ();

        private void Start()
        {
            UnlocksManager.ResetUnlocks();
            
            foreach (var uc in upgradeChoices)
            {
                uc.SetParentUnlockController(this);
                _itemDatas.Add(uc, null);
            }
            
            RerollUpgrades();
        }

        private void RerollUpgrades()
        {
            var lockedItems = UnlocksManager.Instance.GetLockedItems();
            
            Debug.Log("Locked items: " + lockedItems.Count);
            
            foreach (var uc in upgradeChoices)
            {
                var unlockData = lockedItems.Count == 0 ? null : lockedItems[0];
                
                uc.SetFromItemData(unlockData);
                _itemDatas[uc] = unlockData;
                
                lockedItems.Remove(unlockData);
            }
        }

        public void NotifyClick(UpgradeChoiceController source)
        {
            if (_itemDatas[source] != null)
            {
                UnlocksManager.Instance.AddUnlock(_itemDatas[source].item.itemName);
                Debug.Log("Added unlock: " + _itemDatas[source].item.itemName);
            }
            
            RerollUpgrades();
        }
    }
}
