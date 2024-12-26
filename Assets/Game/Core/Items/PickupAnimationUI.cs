using System;
using Cysharp.Threading.Tasks;
using Steamworks;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class PickupAnimationUI : MonoBehaviour
    {

        [SerializeField] private Image image;
        private static GameObject _inventoryButton;
        private static Canvas _canvas;

        private void Awake()
        {
            if(_inventoryButton == null) _inventoryButton = GameObject.FindGameObjectWithTag("InventoryButton");
            if(_canvas == null) _canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
            transform.SetParent(_canvas.transform, false);
        }

        private async UniTask AnimTask()
        {
            var dist = Vector2.Distance(transform.position, _inventoryButton.transform.position);

            var startDist = dist;
            
            while (dist > 1f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                dist = Vector2.Distance(transform.position, _inventoryButton.transform.position);
                transform.position = Vector2.Lerp(transform.position, _inventoryButton.transform.position, Time.deltaTime * 3f);
                image.color = new Color(image.color.r, image.color.g, image.color.b, dist/startDist);
            }
            
            Destroy(gameObject);
        }
        

        public void Set(Vector3 worldPos, Item item)
        {
            image.sprite = item.uiSprite;

            image.material = GameObject.Instantiate(image.material);
            
            image.material.SetColor("_OutlineColor", GroundItem.RarityToColor(item.itemRarity));
            
            //this is the ui element
            RectTransform uiElement = GetComponentInParent<RectTransform>();

            //first you need the RectTransform component of your canvas
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            
            Vector2 viewportPosition = GameManager.MainCamera.WorldToViewportPoint(worldPos);
            Vector2 worldObjectScreenPosition=new Vector2(
                ((viewportPosition.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
                ((viewportPosition.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)));

            //now you can set the position of the ui element
            uiElement.anchoredPosition=worldObjectScreenPosition;
            
            AnimTask().Forget();
        }
    }
}
