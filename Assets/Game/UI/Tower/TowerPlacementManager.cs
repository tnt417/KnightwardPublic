using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev.Game.UI.Tower
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance;
        
        //Editor variables
        [SerializeField] private GameObject towerPlacementIndicator;
        [SerializeField] private Image rangeIndicator;
        //
        private bool Placing => towerPlacementIndicator.activeSelf;
        private Camera _mainCamera;
        private GameObject _selectedTowerPrefab;
        
        public List<GamePhase> placeablePhases = new ();
        private bool CanPlace => placeablePhases.Contains(GameManager.GamePhase);

        private void Start()
        {
            if(Instance != null && Instance != this) Destroy(Instance);
            Instance = this;

            placeablePhases.Add(GamePhase.Arena);
            
            _mainCamera = Camera.main;

            RoomManager.Instance.OnActiveRoomChanged += () =>
            {
                Debug.Log("ABC");
                foreach (var t in FindObjectsOfType<Core.Entities.Towers.Tower>())
                {
                    Debug.Log(t.CurrentParentIdentity);
                    if (t.CurrentParentIdentity.GetComponent<Room>() != null)
                    {
                        t.Pickup(); //Pickup all towers that are in the dungeon, whenever rooms are changed
                    }
                }
            };
        }
        
        private void Update()
        {
            if(!CanPlace) towerPlacementIndicator.SetActive(false);
            if (!Placing) return; //Don't move on if not placing

            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition); //Get the mouse position
            towerPlacementIndicator.transform.position = new Vector3(Mathf.Ceil(mousePos.x), Mathf.Ceil(mousePos.y), 0) - new Vector3(0.5f, 0.5f, 0); //Set the indicator position, snapping to a grid

            if (Input.GetMouseButtonDown(0)) SpawnTower(); //Spawn a tower if placing and click
        }

        public void TogglePlacing(Item item)
        {
            towerPlacementIndicator.SetActive(!Placing); //Toggle the placement indicator activeness
            _selectedTowerPrefab = item.spawnablePrefab; //Update selected prefab
            var tower = _selectedTowerPrefab.GetComponent<Core.Entities.Towers.Tower>(); //Get a reference to the tower of the prefab...
            rangeIndicator.transform.localScale = new Vector3(tower.targetRadius * 2, tower.targetRadius * 2, 1); //...and update the rangeIndicator based on the tower's range.
            var prefabSprite = _selectedTowerPrefab.GetComponentInChildren<SpriteRenderer>(); //Get the prefab's SpriteRenderer
            var indicatorSprite = towerPlacementIndicator.GetComponent<SpriteRenderer>(); //Get the indicator's SpriteRenderer
            indicatorSprite.sprite = item.uiSprite; //Update the indicator's sprite to be the tower's ui sprite
        }
        
        //Spawns a tower at the indicator position and exits from placing mode
        private void SpawnTower()
        {
            GameManager.Instance.CmdSpawnTower(ObjectFinder.GetNameOfPrefab(_selectedTowerPrefab), towerPlacementIndicator.transform.position, Player.LocalInstance.CurrentParentIdentity);
            TowerUIController.Instance.ConfirmPlace();
            towerPlacementIndicator.SetActive(false);
        }
    }
}
