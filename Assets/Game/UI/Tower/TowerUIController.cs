using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Items;
using TonyDev.Game.UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public Dictionary<ItemSlot, Item> Towers = new();
        
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
            Towers.Add(itemSlot, item);
            Towers = Towers.OrderBy(t => t.Value.itemName).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void StartPlacingTower(ItemSlot slot, Item item)
        {
            _selectedTowerSlot = slot;
            TowerPlacementManager.Instance.TogglePlacing(item); //Start/stop placing the prefab specified
        }

        public Sprite GetNextSprite()
        {
            return Towers.Count > 1 ? Towers.First(t => t.Key != _selectedTowerSlot).Value.uiSprite : null;
        }

        public void ConfirmPlace()
        {
            var continuous = TowerPlacementManager.PlaceContinuous;
            
            Towers.Remove(_selectedTowerSlot);
            Destroy(_selectedTowerSlot.gameObject); //Called when clicking while placing. Destroys the UI tower that was just placed.

            if (Towers.Count <= 0 || !continuous) return;
            
            var kvp = Towers.First();
            StartPlacingTower(kvp.Key, kvp.Value);
        }
    }
}
