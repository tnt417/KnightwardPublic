using System.Collections.Generic;
using System.Linq;
using TMPro;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TonyDev.Game.UI.Inventory
{
    public class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private TMP_Text descriptionText;
        private RectTransform _rectTransform;
        private Camera _mainCamera;

        private void Awake()
        {
            _rectTransform = (RectTransform) toggleObject.transform;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
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

            if(gi != null){
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
            descriptionText.text = "<color=white>" + item.itemName + "</color>\n<color=grey>" +
                                   item.GetItemDescription() + "</color>";
        }

        public void Deactivate()
        {
            toggleObject.SetActive(false);
            descriptionText.text = string.Empty;
        }
    }
}