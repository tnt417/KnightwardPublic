using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.UI.Minimap;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager Instance; //Singleton instance

        //Editor variables
        [SerializeField] private Room currentActiveRoom;
        [SerializeField] private RoomGenerator roomGenerator;

        [SerializeField] private GameObject minimapObject;
        //

        private SmoothCameraFollow _smoothCameraFollow;

        //Room-related variables
        public int MapSize => roomGenerator.MapSize; //The shared width/height of the map
        private Vector2Int _currentActiveRoomIndex;

        [SyncVar(hook = nameof(MapHook))] public Map map;

        private void MapHook(Map oldMap, Map newMap)
        {
            OnRoomsChanged?.Invoke();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetMap(Map newMap)
        {
            map = newMap;
        }

        private bool InStartingRoom => _currentActiveRoomIndex == map.StartingRoomPos;
        public bool CanSwitchPhases => InStartingRoom;
        public event Action OnRoomsChanged;

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        }

        private void Start()
        {
            _smoothCameraFollow = FindObjectOfType<SmoothCameraFollow>(); //Initialize the camera follow variables
            
            OnRoomsChanged += MinimapManager.Instance.UpdateMinimap;
            OnRoomsChanged += DoDoorClosing;
            OnRoomsChanged?.Invoke();
        }

        private void Update()
        {
            minimapObject.SetActive(GameManager.GamePhase == GamePhase.Dungeon);
            if (Player.LocalInstance != null)
                MinimapManager.Instance.SetPlayerPosition(Player.LocalInstance.transform.position,
                    roomGenerator.roomOffset);
        }

        [Server]
        public void GenerateRooms()
        {
            GameConsole.Log("Generating rooms...");
            CmdSetMap(roomGenerator.Generate()); //Randomly generate rooms
        }

        public void ShiftActiveRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            Player.LocalInstance.playerMovement.DoMovement = false;
            StartCoroutine(MovePlayerToNextRoom(direction));
        }

        private IEnumerator MovePlayerToNextRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            TransitionController.Instance.FadeInOut(); //Transition to make it less jarring.
            yield return
                new WaitUntil(() =>
                    TransitionController.Instance.OutTransitionDone); //Wait until the transition is over

            var dx = 0; //delta x
            var dy = 0; //delta y

            switch (direction) //Set the delta variables based on the direction
            {
                case Direction.Left:
                    dx = -1;
                    break;
                case Direction.Right:
                    dx = 1;
                    break;
                case Direction.Up:
                    dy = 1;
                    break;
                case Direction.Down:
                    dy = -1;
                    break;
            }

            var room = map.Rooms[_currentActiveRoomIndex.x + dx, _currentActiveRoomIndex.y + dy];

            SetActiveRoom(_currentActiveRoomIndex.x + dx,
                _currentActiveRoomIndex.y + dy); //Set the active room to the new room

            //Move the player into the newly activated room
            var playerTransform = Player.LocalInstance.transform;
            playerTransform.position = room.GetSpawnpoint(direction);
            Player.LocalInstance.playerMovement.DoMovement = true;
            //
        }

        public void SetActiveRoom(int x, int y)
        {
            var newRoom = map.Rooms[x, y]; //Get the new room from the array
            if (newRoom == null) return; //If it's null, do nothing

            DeactivateAllRooms(); //Deactivate all rooms.

            currentActiveRoom = newRoom; //Update currentActiveRoom variable
            currentActiveRoom.gameObject.SetActive(true); //Activate the new room
            MinimapManager.Instance.UncoverRoom(new Vector2Int(x, y)); //Uncover the room on the minimap
            _smoothCameraFollow.SetCameraBounds(currentActiveRoom.RoomRect); //Update the camera bounds
            _currentActiveRoomIndex = new Vector2Int(x, y); //Update currentActiveRoomIndex variable
            
            Player.LocalInstance.CmdSetParentIdentity(newRoom.netIdentity);
        }

        public void DeactivateAllRooms()
        {
            if (map.Rooms == null) return;

            foreach (var r in map.Rooms)
            {
                if (r != null)
                {
                    r.gameObject.SetActive(false);
                }
            }
        }

        //Performs all actions necessary to disable the room phase.
        public void DeactivateRoomPhase()
        {
            _currentActiveRoomIndex = Vector2Int.zero;
            if (_smoothCameraFollow != null) _smoothCameraFollow.SetCameraBounds(Rect.zero);

            Player.LocalInstance.CmdSetParentIdentity(null);
            
            DeactivateAllRooms();
            currentActiveRoom = null;
        }

        private void DoDoorClosing()
        {
            if (map.Rooms == null)
            {
                Debug.LogWarning("Room array is null!");
                return;
            }

            for (var i = 0; i < roomGenerator.MapSize; i++) //Loop through all the rooms
            {
                for (var j = 0; j < roomGenerator.MapSize; j++)
                {
                    if (map.Rooms[i, j] == null) continue; //If there's no room, move on

                    //Set open directions based on room adjacency
                    var openDirections = new List<Direction>();
                    if (CheckIfRoomExistsAndHasDoor(i - 1, j, Direction.Right)) openDirections.Add(Direction.Left);
                    if (CheckIfRoomExistsAndHasDoor(i + 1, j, Direction.Left)) openDirections.Add(Direction.Right);
                    if (CheckIfRoomExistsAndHasDoor(i, j - 1, Direction.Up)) openDirections.Add(Direction.Down);
                    if (CheckIfRoomExistsAndHasDoor(i, j + 1, Direction.Down)) openDirections.Add(Direction.Up);
                    map.Rooms[i, j].SetOpenDirections(openDirections);
                    //
                }
            }
        }

        private bool CheckIfRoomExistsAndHasDoor(int x, int y, Direction direction)
        {
            if (x < 0 || y < 0 || x > roomGenerator.MapSize - 1 || y > roomGenerator.MapSize - 1) return false;
            return map.Rooms[x, y] != null && map.Rooms[x, y].GetDoorDirections().Contains(direction);
        }

        public void ResetRooms()
        {
            foreach (var r in map.Rooms) //Destroy all rooms...
                if (r != null)
                    Destroy(r.gameObject);

            MinimapManager.Instance.Reset();
            roomGenerator.Reset();
            DeactivateAllRooms();
        }

        public void TeleportPlayerToStart()
        {
            Player.LocalInstance.transform.position = map.StartingRoom.transform.position;
            SetActiveRoom(map.StartingRoomPos.x, map.StartingRoomPos.y);
        }
    }
}