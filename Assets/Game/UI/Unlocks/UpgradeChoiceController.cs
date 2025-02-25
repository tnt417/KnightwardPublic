using TMPro;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Unlocks
{
    public class UpgradeChoiceController : MonoBehaviour
    {
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescriptionText;
        [SerializeField] private Image itemImage;

        private UnlockUIController _unlockUI;

        public void SetParentUnlockController(UnlockUIController unlockUI)
        {
            _unlockUI = unlockUI;
        }
        
        public void SetFromItemData(ItemData itemData)
        {
            itemNameText.text = itemData?.item.itemName;
            itemDescriptionText.text = itemData?.item.itemDescription;
            itemImage.sprite = itemData?.item.uiSprite;
        }

        public void OnClick()
        {
            _unlockUI.NotifyClick(this);
        }
    }
}
