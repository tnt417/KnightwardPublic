using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Inventory
{
    public class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform containerTransform;
        [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private GameObject compareToggleObject;
        [SerializeField] private Image toggleObjectPanel;
        [SerializeField] private Image compareObjectPanel;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text compareDescriptionText;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private Core.Entities.Towers.Tower _lastHoveredTower;
        
        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            containerTransform.position = Mouse.current.position.ReadValue();

            foreach (var slot in raycastResults.Select(curRaycastResult =>
                curRaycastResult.gameObject.GetComponent<ItemSlot>()))
            {
                if (slot == null) continue;
                Set(slot.Item);
                return;
            }

            var mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

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

            if (GameManager.EntitiesReadonly.FirstOrDefault(ge =>
                    ge is Core.Entities.Towers.Tower && Vector2.Distance(mousePos, ge.transform.position) < 0.5f && ge.CurrentParentIdentity == Player.LocalInstance.CurrentParentIdentity) is
                Core.Entities.Towers.Tower t)
            {
                if (t != _lastHoveredTower && _lastHoveredTower != null)
                {
                    _lastHoveredTower.NotifyMouseUnhover();
                }
                
                t.NotifyMouseHover();
                Set(t.myItem);
                _lastHoveredTower = t;
                return;
            }

            if (_lastHoveredTower != null)
            {
                _lastHoveredTower.NotifyMouseUnhover();
                _lastHoveredTower = null;
            }

            Deactivate();
        }

        public void Set(Item item)
        {
            if (item == null) return;

            var replacement = PlayerInventory.Instance.GetSwap(item);

            toggleObjectPanel.color = GroundItem.RarityToColor(item.itemRarity);
            
            compareToggleObject.SetActive(replacement != null);
            if (replacement != null) compareObjectPanel.color = GroundItem.RarityToColor(replacement.itemRarity);

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

                pivotY = containerTransform.position.y + containerTransform.rect.height < _mainCamera.pixelRect.yMin
                    ? 1
                    : 0;

                containerTransform.pivot = new Vector2(pivotX, pivotY);
            }

            containerTransform.gameObject.SetActive(true);

            descriptionText.text = GetItemDescriptionText(item);

            if (replacement != null) compareDescriptionText.text = GetItemDescriptionText(replacement);
        }

        private string GetItemDescriptionText(Item item)
        {
            return "<color=white><size=32>" + item.itemName + "</size></color>"
                   + "  <color=#" + ColorUtility.ToHtmlStringRGBA(GroundItem.RarityToColor(item.itemRarity)) + ">" +
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