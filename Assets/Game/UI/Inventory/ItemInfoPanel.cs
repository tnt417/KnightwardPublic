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
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image backgroundImage;
        private RectTransform _rectTransform;
        private Camera _mainCamera;

        private void Start()
        {
            _rectTransform = (RectTransform) toggleObject.transform;
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

            _rectTransform.position = Input.mousePosition;

            foreach (var itemSlot in raycastResults.Select(curRaycastResult =>
                curRaycastResult.gameObject.GetComponent<ItemSlot>()))
            {
                if (itemSlot == null) continue;
                Set(itemSlot.Item);
                return;
            }

            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var rHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("UI"));
            var t = rHit.transform;
            GroundItem gi = null;

            if (t != null) gi = t.gameObject.GetComponent<GroundItem>();

            if (gi != null)
            {
                Set(gi.Item);
                return;
            }

            Deactivate();
        }

        public void Set(Item item)
        {
            if (!toggleObject.activeSelf)
            {
                var pivotX = 0;
                var pivotY = 0;

                _rectTransform.pivot = new Vector2(0, 0);

                if (_rectTransform.rect.xMax > _mainCamera.rect.xMax)
                {
                    pivotX = 1;
                }

                if (_rectTransform.rect.xMax < _mainCamera.rect.xMax)
                {
                    pivotX = 0;
                }

                if (_rectTransform.rect.yMax > _mainCamera.rect.yMax)
                {
                    pivotY = 1;
                }

                if (_rectTransform.rect.xMax > _mainCamera.rect.xMax)
                {
                    pivotY = 0;
                }

                _rectTransform.pivot = new Vector2(pivotX, pivotY);
            }

            if (item == null) return;
            toggleObject.SetActive(true);

            var rarityColor = item.itemRarity switch
            {
                ItemRarity.Common => "grey",
                ItemRarity.Uncommon => "green",
                ItemRarity.Rare => "yellow",
                ItemRarity.Unique => "red",
                _ => throw new ArgumentOutOfRangeException()
            };

            descriptionText.text = "<color=white><size=24>" + item.itemName + "</size></color>"
                                   + "  <color=" + rarityColor + ">" +
                                   Enum.GetName(typeof(ItemRarity), item.itemRarity) + "</color>"
                                   + "\n<color=#c0c0c0ff>" + item.GetItemDescription() + "</color>";

            // var backColor = item.itemRarity switch
            // {
            //     ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f, 0.5f),
            //     ItemRarity.Uncommon => new Color(0.13f, 0.8f, 0f, 0.5f),
            //     ItemRarity.Rare => new Color(0.8f, 0.6f, 0f, 0.5f),
            //     ItemRarity.Unique => new Color(0.8f, 0.018f, 0f, 0.5f),
            //     _ => throw new ArgumentOutOfRangeException()
            // };

            //backgroundImage.color = backColor;
        }

        public void Deactivate()
        {
            toggleObject.SetActive(false);
            descriptionText.text = string.Empty;
        }
    }
}