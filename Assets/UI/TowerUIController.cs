using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Towers;
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

        private UITower _selectedUITower;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }
        
        public void AddTower(GameObject prefab)
        {
            var go = Instantiate(uiTowerPrefab, uiTowerGrid.transform);
            go.SendMessage("Set", prefab); //Tell the UITower what tower it is
        }

        public void StartPlacingTower(UITower tower, GameObject prefab)
        {
            TowerPlacementManager.Instance.TogglePlacing(prefab); //Start/stop placing the prefab specified
            _selectedUITower = tower;
        }

        public void ConfirmPlace()
        {
            Destroy(_selectedUITower.gameObject); //Called when clicking while placing. Destroys the UI tower that was just placed.
        }
    }
}
