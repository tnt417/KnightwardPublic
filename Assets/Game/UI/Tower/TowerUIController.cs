using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Items;
using TonyDev.Game.UI.Inventory;
using UnityEngine;

namespace TonyDev.Game.UI.Tower
{
    public class TowerUIController : MonoBehaviour
    {
        public static TowerUIController Instance;
        //Editor variables
        [SerializeField] private GameObject uiTowerPrefab;
        [SerializeField] private GameObject uiTowerGrid;
        //

        private ItemSlot _selectedTowerSlot;

        public List<Item> towers;
        
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
            towers.Add(item);
            towers = towers.OrderBy(t => t.itemName).ToList();
        }

        public void StartPlacingTower(ItemSlot slot, Item item)
        {
            TowerPlacementManager.Instance.TogglePlacing(item); //Start/stop placing the prefab specified
            _selectedTowerSlot = slot;
        }

        public void ConfirmPlace()
        {
            towers.Remove(_selectedTowerSlot.Item);
            Destroy(_selectedTowerSlot.gameObject); //Called when clicking while placing. Destroys the UI tower that was just placed.
        }
    }
}
