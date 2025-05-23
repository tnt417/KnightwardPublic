using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.UI.Tower
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance;

        //Editor variables
        [SerializeField] private GameObject towerPlacementIndicator;

        [SerializeField] private SpriteRenderer rangeIndicator;
        
        [SerializeField] private SpriteRenderer nextIndicator;
        [SerializeField] private TMP_Text nextText;

        //
        public bool BlockAttacksFromTowerPlacement { get; private set; }
        public bool Placing => towerPlacementIndicator.activeSelf;
        private Camera _mainCamera;
        private Item _selectedTowerItem;
        private SpriteRenderer _indicatorSprite;
        private Collider2D _indicatorCollider;

        public List<GamePhase> placeablePhases = new();
        private bool CanPlace => placeablePhases.Contains(GameManager.GamePhase);
        public static bool PlaceContinuous => Keyboard.current.shiftKey.isPressed;

        private IEnumerable<Tilemap> _floorTilemaps;
        
        private void Start()
        {
            Instance = this;

            _floorTilemaps = FindObjectsOfType<Tilemap>().Where(tm => tm.gameObject.CompareTag("Floor"));
            
            placeablePhases.Add(GamePhase.Arena);

            _mainCamera = Camera.main;

            RoomManager.OnActiveRoomChanged += PickupDungeonTowers;
        }

        public void PickupDungeonTowers()
        {
            foreach (var t in FindObjectsOfType<Core.Entities.Towers.Tower>())
            {
                if (t.CurrentParentIdentity != null)
                {
                    t.CmdRequestPickup(); //Pickup all towers that are in the dungeon, whenever rooms are changed
                }
            }
        }

        private void OnDestroy()
        {
            RoomManager.OnActiveRoomChanged -= PickupDungeonTowers;
        }

        private Vector2 _indicatorVelocity = Vector3.zero;
        private Vector2 _indicatorVelocityMouse = Vector3.zero;

        private float _lastPlacingTime;
        
        private void Update()
        {
            if (!CanPlace) towerPlacementIndicator.SetActive(false);
            if (!Placing)
            {
                // Allow attacks 0.25 seconds after placing
                if (Time.time - _lastPlacingTime > 0.25f && BlockAttacksFromTowerPlacement)
                    BlockAttacksFromTowerPlacement = false;
                return; //Don't move on if not placing
            }

            _lastPlacingTime = Time.time;
            BlockAttacksFromTowerPlacement = true;
            
            rangeIndicator.transform.localScale = Vector2.SmoothDamp(rangeIndicator.transform.localScale,
                new Vector2(_selectedTower.targetRange * 2, _selectedTower.targetRange * 2), ref _indicatorVelocity, 0.03f );
            
            nextText.text = PlaceContinuous ? "NEXT" : "[SHIFT]";

            var mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //Get the mouse position
            towerPlacementIndicator.transform.position = Vector2.SmoothDamp(towerPlacementIndicator.transform.position,
                new Vector2(Mathf.Ceil(mousePos.x), Mathf.Ceil(mousePos.y)) -
                new Vector2(0.5f, 0.5f), ref _indicatorVelocityMouse, 0.01f
            ); //Set the indicator position, snapping to a grid

            if (Mouse.current.leftButton.wasPressedThisFrame)
                SpawnTower().Forget(); //Spawn a tower if placing and click
            if (Mouse.current.rightButton.wasPressedThisFrame) TogglePlacing(null);
        }

        private void FixedUpdate()
        {
            if (!Placing) return;
            
            var mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //Get the mouse position
            var valid = ValidSpot(new Vector2(Mathf.Ceil(mousePos.x), Mathf.Ceil(mousePos.y)) -
                                  new Vector2(0.5f, 0.5f));

            var color = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            rangeIndicator.color = color;
            color.a = 1;
            _indicatorSprite.color = color;
        }
        
        private Core.Entities.Towers.Tower _selectedTower;
        
        public void TogglePlacing(Item item)
        {
            towerPlacementIndicator.SetActive(!Placing); //Toggle the placement indicator activeness

            _selectedTowerItem = item; //Update selected prefab

            if (_selectedTowerItem == null) return;

            var tower = _selectedTowerItem.SpawnablePrefab
                .GetComponent<Core.Entities.Towers.Tower>(); //Get a reference to the tower of the prefab...

            _selectedTower = tower;

            _indicatorSprite =
                towerPlacementIndicator.GetComponentInChildren<SpriteRenderer>(); //Get the indicator's SpriteRenderer
            _indicatorCollider = towerPlacementIndicator.GetComponent<Collider2D>();

            nextIndicator.sprite = TowerUIController.Instance.GetNextSprite();
            nextIndicator.gameObject.SetActive(nextIndicator.sprite != null);

            _indicatorSprite.sprite = item.uiSprite; //Update the indicator's sprite to be the tower's ui sprite
        }

        private bool ValidSpot(Vector2 pos)
        {
            //Don't place on top of occupied spots
            //if (GameManager.Instance.OccupiedTowerSpots.Contains(vec2Int)) return false; //Commented out to allow for tower swapping

            if (Vector2.Distance(Player.LocalInstance.transform.position, pos) > 10f) return false;
            
            //Only place on floor tiles
            var floorAtSpot = _floorTilemaps.Any(tm => tm.GetTile(tm.WorldToCell(pos)) != null);

            var distXFromCrystal = Mathf.Abs(pos.x - Crystal.Instance.transform.position.x);
            var distYFromCrystal = Mathf.Abs(pos.y - Crystal.Instance.transform.position.y);

            if (distXFromCrystal <= 2 && distYFromCrystal <= 1) return false;

            if (!floorAtSpot) return false;

            // Jan 15 2025: removed this because it is really annoying when trying to place with enemies
            //Can't place if stuck in crystal or other non-trigger collider
            // var contacts = new Collider2D[100];
            // _indicatorCollider.GetContacts(contacts);
            //
            // if (contacts.Any(c => c != null && !c.isTrigger)) return false;

            return true;
        }
        
        //Spawns a tower at the indicator position and exits from placing mode
        private async UniTask SpawnTower()
        {
            var mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //Get the mouse position
            
            var pos = new Vector2(Mathf.Ceil(mousePos.x), Mathf.Ceil(mousePos.y)) -
                      new Vector2(0.5f, 0.5f);

            if (!ValidSpot(pos)) return;

            var success = await GameManager.Instance.SpawnTowerTask(_selectedTowerItem,
                towerPlacementIndicator.transform.position, Player.LocalInstance.CurrentParentIdentity).Preserve();

            SoundManager.PlaySound("interact", 0.5f, pos);

            if (!success) return;
            
            towerPlacementIndicator.SetActive(false);
            
            TowerUIController.Instance.ConfirmPlace();
        }
    }
}