using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace TonyDev.Game.Level.Rooms
{
    public class MinimapManager : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private GridLayoutGroup uiRoomGridLayout;
        [SerializeField] private GameObject minimapRoomPrefab;
        [SerializeField] private Sprite unknownRoomSprite;
        //
    
        public static MinimapManager Instance;
    
        private RoomManager _roomManager;
        private Room[,] _rooms;
        private GameObject[,] _uiRoomObjects;
        private int[,] _discoveredRooms;

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        }

        public void Reset()
        {
            foreach (var go in _uiRoomObjects)
            {
                Destroy(go);
            }
        }
    
        public void UpdateMinimap()
        {
            _roomManager = FindObjectOfType<RoomManager>();
            _rooms = _roomManager.Rooms;
            _discoveredRooms = new int[_roomManager.MapSize, _roomManager.MapSize];
            _uiRoomObjects = new GameObject[_roomManager.MapSize, _roomManager.MapSize];

            uiRoomGridLayout.constraintCount = _roomManager.MapSize;
        
            for (var i = 0; i < _roomManager.MapSize; i++)
            {
                for (var j = 0; j < _roomManager.MapSize; j++)
                {
                    var go = Instantiate(minimapRoomPrefab, uiRoomGridLayout.transform);
                    _uiRoomObjects[i, j] = go;
                    var symbolImage = go.GetComponentsInChildren<Image>().FirstOrDefault(img => img.CompareTag("MinimapIcon"));
                
                    if (_rooms[i, j] != null) continue;
                
                    go.GetComponent<Image>().enabled = false;
                    if (symbolImage != null) symbolImage.enabled = false;
                }
            }
        }

        public void UncoverRoom(Vector2Int position)
        {
            _discoveredRooms[position.x, position.y] = 1;
            UpdateMinimapSymbols();
        }

        private void UpdateMinimapSymbols()
        {
            for (var i = 0; i < _roomManager.MapSize; i++) //Loop through every room
            {
                for (var j = 0; j < _roomManager.MapSize; j++)
                {
                    var symbolImage = _uiRoomObjects[i,j].GetComponentsInChildren<Image>().FirstOrDefault(img => img.CompareTag("MinimapIcon")); //Get the symbol image
                
                    if (symbolImage == null) continue; //Check for null
                
                    if (_discoveredRooms[i, j] == 0) //If room is undiscovered...
                    {
                        symbolImage.sprite = unknownRoomSprite; //...set symbol to unknown symbol
                        continue;
                    }
                
                    if (_rooms[i, j] == null) symbolImage.enabled = false; //If the room is null, turn off the UI image
                    else symbolImage.sprite = _rooms[i, j].minimapIcon; //If the room exists and is turned on, set its map icon.
                }
            }
        }

        public void SetPlayerPosition(Vector2 playerPos, Vector2 roomOffset)
        {
            //MATH EXPLANATION: Each room grid is 100x100 in the UI. playerPos divided by roomOffset gets the player's position in terms of
            //                  room coordinates. Just multiply that by 100px to convert it to the UI room coordinates, subtracting half of the minimap's dimensions to center it,
            //                  and it works perfectly. playerPos is negative since it isn't the player moving, it is the world moving around it.
            ((RectTransform) uiRoomGridLayout.transform).localPosition =
                100f * (-playerPos / roomOffset) - new Vector2(200f, 200f);
        }
    }
}
