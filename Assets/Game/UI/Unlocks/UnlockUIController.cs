using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.UI.Unlocks
{
    public class UnlockUIController : MonoBehaviour
    {
        
        [SerializeField] private UpgradeChoiceController[] upgradeChoices;
        
        private Dictionary<UpgradeChoiceController, ItemData> _itemDatas = new ();

        public void GoBackToMainMenu()
        {
            TransitionToMainMenu().Forget();
        }
        
        private async UniTask TransitionToMainMenu()
        {
            TransitionController.Instance.FadeOut();
            await UniTask.Delay(TimeSpan.FromSeconds(TransitionController.FadeOutTimeSeconds));
            await SceneManager.LoadSceneAsync("MainMenuScene");
            TransitionController.Instance.FadeIn();
        }
        
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
