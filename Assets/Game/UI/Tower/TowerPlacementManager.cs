using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
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
        private Item _selectedTowerItem;
        private SpriteRenderer _indicatorSprite;
        private Collider2D _indicatorCollider;

        public List<GamePhase> placeablePhases = new();
        private bool CanPlace => placeablePhases.Contains(GameManager.GamePhase);

        private void Start()
        {
            Instance = this;

            placeablePhases.Add(GamePhase.Arena);

            _mainCamera = Camera.main;

            RoomManager.Instance.OnActiveRoomChanged += () =>
            {
                foreach (var t in FindObjectsOfType<Core.Entities.Towers.Tower>())
                {
                    if (t.CurrentParentIdentity != null)
                    {
                        t.CmdRequestPickup(); //Pickup all towers that are in the dungeon, whenever rooms are changed
                    }
                }
            };
        }

        private void Update()
        {
            if (!CanPlace) towerPlacementIndicator.SetActive(false);
            if (!Placing) return; //Don't move on if not placing

            var mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //Get the mouse position
            towerPlacementIndicator.transform.position =
                new Vector3(Mathf.Ceil(mousePos.x), Mathf.Ceil(mousePos.y), 0) -
                new Vector3(0.5f, 0.5f, 0); //Set the indicator position, snapping to a grid

            if (Mouse.current.leftButton.wasPressedThisFrame)
                SpawnTower().Forget(); //Spawn a tower if placing and click
            if (Mouse.current.rightButton.wasPressedThisFrame) TogglePlacing(null);
        }

        private void FixedUpdate()
        {
            if (!Placing) return;

            var valid = ValidSpot(towerPlacementIndicator.transform.position);

            var color = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            rangeIndicator.color = color;
            color.a = 1;
            _indicatorSprite.color = color;
        }

        public void TogglePlacing(Item item)
        {
            towerPlacementIndicator.SetActive(!Placing); //Toggle the placement indicator activeness

            _selectedTowerItem = item; //Update selected prefab

            if (_selectedTowerItem == null) return;

            var tower = _selectedTowerItem.SpawnablePrefab
                .GetComponent<Core.Entities.Towers.Tower>(); //Get a reference to the tower of the prefab...

            rangeIndicator.transform.localScale =
                new Vector3(tower.targetRadius * 2, tower.targetRadius * 2,
                    1); //...and update the rangeIndicator based on the tower's range.

            _indicatorSprite =
                towerPlacementIndicator.GetComponent<SpriteRenderer>(); //Get the indicator's SpriteRenderer
            _indicatorCollider = towerPlacementIndicator.GetComponent<Collider2D>();

            _indicatorSprite.sprite = item.uiSprite; //Update the indicator's sprite to be the tower's ui sprite
        }

        private bool ValidSpot(Vector2 pos)
        {
            //Don't place on top of occupied spots
            //if (GameManager.Instance.OccupiedTowerSpots.Contains(vec2Int)) return false; //Commented out to allow for tower swapping

            //Only place on floor tiles
            var floorAtSpot = FindObjectsOfType<Tilemap>().Where(tm => tm.gameObject.CompareTag("Floor"))
                .Any(tm => tm.GetTile(tm.WorldToCell(pos)) != null);

            if (!floorAtSpot) return false;

            //Can't place if stuck in crystal or other non-trigger collider
            var contacts = new Collider2D[100];
            _indicatorCollider.GetContacts(contacts);

            if (contacts.Any(c => c != null && !c.isTrigger)) return false;

            return true;
        }

        //Spawns a tower at the indicator position and exits from placing mode
        private async UniTask SpawnTower()
        {
            var pos = (Vector2) towerPlacementIndicator.transform.position;

            if (!ValidSpot(pos)) return;

            var success = await GameManager.Instance.SpawnTowerTask(_selectedTowerItem,
                towerPlacementIndicator.transform.position, Player.LocalInstance.CurrentParentIdentity).Preserve();

            SoundManager.PlaySound("interact", 0.5f, pos);

            if (!success) return;

            TowerUIController.Instance.ConfirmPlace();
            towerPlacementIndicator.SetActive(false);
        }
    }
}