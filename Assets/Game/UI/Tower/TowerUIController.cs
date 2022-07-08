using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Towers;
using TonyDev.Game.Core.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.UI
{
    public class TowerUIController : MonoBehaviour
    {
        public static TowerUIController Instance;
        //Editor variables
        [SerializeField] private GameObject uiTowerPrefab;
        [SerializeField] private GameObject uiTowerGrid;
        //

        private ItemSlot _selectedTowerSlot;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            gameObject.SetActive(false);
        }
        
        public void AddTower(Item item)
        {
            var itemSlot = Instantiate(uiTowerPrefab, uiTowerGrid.transform).GetComponent<ItemSlot>();
            itemSlot.Item = item;
        }

        public void StartPlacingTower(ItemSlot slot, Item item)
        {
            TowerPlacementManager.Instance.TogglePlacing(item); //Start/stop placing the prefab specified
            _selectedTowerSlot = slot;
        }

        public void ConfirmPlace()
        {
            Destroy(_selectedTowerSlot.gameObject); //Called when clicking while placing. Destroys the UI tower that was just placed.
        }
    }
}
