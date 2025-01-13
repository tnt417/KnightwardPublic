using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace TonyDev.Game.UI.Minimap
{
    public class MinimapManager : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private GridLayoutGroup uiRoomGridLayout;
        [SerializeField] private GameObject minimapRoomPrefab;

        [SerializeField] private Sprite unknownRoomSprite;
        [SerializeField] private Sprite commonRoomSprite;
        [SerializeField] private Sprite uncommonRoomSprite;
        [SerializeField] private Sprite specialRoomSprite;
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
            if (_uiRoomObjects == null) return;
            foreach (var go in _uiRoomObjects)
            {
                Destroy(go);
            }
        }

        public void UpdateMinimap()
        {
            _roomManager = FindObjectOfType<RoomManager>();
            _rooms = _roomManager.Map.Rooms;

            if (_rooms == null) return;

            Reset();

            _discoveredRooms = new int[_roomManager.MapSize, _roomManager.MapSize];
            _uiRoomObjects = new GameObject[_roomManager.MapSize, _roomManager.MapSize];

            uiRoomGridLayout.constraintCount = _roomManager.MapSize;

            for (var i = 0; i < _roomManager.MapSize; i++)
            {
                for (var j = 0; j < _roomManager.MapSize; j++)
                {
                    if (_rooms[i, j] == null) continue;

                    var go = Instantiate(minimapRoomPrefab, uiRoomGridLayout.transform);
                    var rectTransform = (RectTransform) go.transform;
                    rectTransform.localPosition = new Vector2((i - _roomManager.MapSize / 2) * 110,
                        (j - _roomManager.MapSize / 2) * 110);
                    _uiRoomObjects[i, j] = go;
                    var symbolImage = go.GetComponentsInChildren<Image>()
                        .FirstOrDefault(img => img.CompareTag("MinimapIcon"));
                }
            }

            UpdateMinimapSymbols();
        }

        public void UpdatePlayerCount(Vector2Int pos, int playerCount)
        {
            var playerImages = _uiRoomObjects[pos.x, pos.y].GetComponentsInChildren<Image>()
                .Where(img => img.CompareTag("MinimapAlly")).ToArray();

            for (var k = 0; k < playerImages.Length; k++)
            {
                if (RoomManager.Instance.currentActiveRoom == _rooms[pos.x, pos.y])
                {
                    playerImages[k].enabled = false;
                }
                else
                {
                    playerImages[k].enabled = playerCount > k;
                }
            }
        }

        public void UpdateOpenDirections(Vector2Int pos, List<Direction> openDirections)
        {
            var connectImages = _uiRoomObjects[pos.x, pos.y].GetComponentsInChildren<Image>()
                .Where(img => img.transform.parent.name == "Connections");

            foreach (var t in connectImages)
            {
                t.enabled = t.name == "Left" && openDirections.Contains(Direction.Left)
                            || t.name == "Up" && openDirections.Contains(Direction.Up)
                            || t.name == "Right" && openDirections.Contains(Direction.Right)
                            || t.name == "Down" && openDirections.Contains(Direction.Down);
            }
        }

        public void UncoverRoom(Vector2Int position)
        {
            _discoveredRooms[position.x, position.y] = 1;

            var roomObj = _uiRoomObjects[position.x, position.y];

            if (roomObj == null) return;

            var symbolImage = roomObj.GetComponentsInChildren<Image>()
                .FirstOrDefault(img => img.CompareTag("MinimapIcon"));

            if (symbolImage == null)
            {
                //Debug.LogWarning("Room image missing!");
                return;
            }

            if (_rooms[position.x,position.y].CompareTag("StartRoom"))
            {
                symbolImage.sprite = _rooms[position.x, position.y].minimapIcon;
                return;
            }

            symbolImage.sprite = _rooms[position.x, position.y].minimapIcon;
        }

        [GameCommand(Keyword = "revealmap", PermissionLevel = PermissionLevel.Cheat, SuccessMessage = "Map revealed.")]
        public static void UncoverAllRooms()
        {
            for (var i = 0; i < RoomManager.Instance.MapSize; i++)
            {
                for (var j = 0; j < RoomManager.Instance.MapSize; j++)
                {
                    MinimapManager.Instance.UncoverRoom(new Vector2Int(i, j));
                }
            }
        }

        private void UpdateMinimapSymbols()
        {
            for (var i = 0; i < _roomManager.MapSize; i++) //Loop through every room
            {
                for (var j = 0; j < _roomManager.MapSize; j++)
                {
                    if (_uiRoomObjects[i, j] == null) continue;

                    var symbolImage = _uiRoomObjects[i, j].GetComponentsInChildren<Image>()
                        .FirstOrDefault(img => img.CompareTag("MinimapIcon")); //Get the symbol image

                    if (symbolImage == null) continue; //Check for null

                    if (_discoveredRooms[i, j] == 0) //If room is undiscovered...
                    {
                        symbolImage.sprite = unknownRoomSprite; //...set symbol to unknown symbol
                        continue;
                    }

                    if (_rooms[i, j] == null) symbolImage.enabled = false; //If the room is null, turn off the UI image
                    else
                        symbolImage.sprite =
                            _rooms[i, j].minimapIcon; //If the room exists and is turned on, set its map icon.
                }
            }
        }

        public void SetPlayerPosition(Vector2 playerPosNormalized)
        {
            ((RectTransform) uiRoomGridLayout.transform).localPosition =
                110 * -playerPosNormalized - new Vector2(110 * _roomManager.currentActiveRoomIndex.x,
                    110 * _roomManager.currentActiveRoomIndex.y) + new Vector2(950, 955);
        }
    }
}