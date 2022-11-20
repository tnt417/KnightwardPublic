using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Inventory
{
    public class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform containerTransform;
        [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private GameObject compareToggleObject;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text compareDescriptionText;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            containerTransform.position = Input.mousePosition;

            foreach (var slot in raycastResults.Select(curRaycastResult =>
                curRaycastResult.gameObject.GetComponent<ItemSlot>()))
            {
                if (slot == null) continue;
                Set(slot.Item);
                return;
            }

            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var rHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("UI")).transform;

            if (rHit != null)
            {
                var gi = rHit.gameObject.GetComponent<GroundItem>();
                
                if (gi != null)
                {
                    Set(gi.Item);
                    return;
                }
            }
            
            var rHit2 = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity).transform;

            if (rHit2 != null)
            {
                var tower = rHit2.gameObject.GetComponent<Core.Entities.Towers.Tower>();

                if (tower != null)
                {
                    Set(tower.myItem);
                    return;
                }
            }

            Deactivate();
        }

        public void Set(Item item)
        {
            if (item == null) return;
            
            var replacement = PlayerInventory.Instance.GetSwap(item);
            
            compareToggleObject.SetActive(replacement != null);

            if (!containerTransform.gameObject.activeSelf)
            {
                var pivotX = 0;
                var pivotY = 0;
                
                horizontalLayoutGroup.reverseArrangement = false;

                if (containerTransform.position.x + containerTransform.rect.width > _mainCamera.pixelRect.xMax)
                {
                    pivotX = 1;
                    horizontalLayoutGroup.reverseArrangement = true;
                }
                else
                {
                    pivotX = 0;
                }

                pivotY = containerTransform.position.y + containerTransform.rect.height < _mainCamera.pixelRect.yMin ? 1 : 0;

                containerTransform.pivot = new Vector2(pivotX, pivotY);
            }

            containerTransform.gameObject.SetActive(true);

            descriptionText.text = GetItemDescriptionText(item);

            if(replacement != null) compareDescriptionText.text = GetItemDescriptionText(replacement);
        }

        private string GetItemDescriptionText(Item item)
        {
            var rarityColor = item.itemRarity switch
            {
                ItemRarity.Common => "grey",
                ItemRarity.Uncommon => "green",
                ItemRarity.Rare => "yellow",
                ItemRarity.Unique => "red",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return "<color=white><size=24>" + item.itemName + "</size></color>"
                   + "  <color=" + rarityColor + ">" +
                   Enum.GetName(typeof(ItemRarity), item.itemRarity) + "</color>"
                   + "\n<color=#c0c0c0ff>" + item.GetItemDescription() + "</color>";
        }

        public void Deactivate()
        {
            containerTransform.gameObject.SetActive(false);
            descriptionText.text = string.Empty;
        }
    }
}