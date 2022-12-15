using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items.Money;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.UI.Minimap;
using UnityEngine;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager Instance; //Singleton instance

        public Room currentActiveRoom;

        //Editor variables
        [SerializeField] private RoomGenerator roomGenerator;

        [SerializeField] private GameObject minimapObject;
        //

        private SmoothCameraFollow _smoothCameraFollow;

        //Room-related variables
        public int MapSize => roomGenerator.MapSize; //The shared width/height of the map
        private Vector2Int _currentActiveRoomIndex;

        [SyncVar(hook = nameof(MapHook))] public Map map;

        [Command(requiresAuthority = false)]
        private void CmdSetMap(Map newMap)
        {
            map = new Map(newMap.Rooms, newMap.StartingRoomPos);
        }

        private const float RequestRate = 0.25f;
        private double _nextRequestTime;

        private void Update()
        {
            minimapObject.SetActive(GameManager.GamePhase == GamePhase.Dungeon);
            if (Player.LocalInstance != null)
                MinimapManager.Instance.SetPlayerPosition(Player.LocalInstance.transform.position,
                    roomGenerator.roomOffset);
        }

        // [Command(requiresAuthority = false)]
        // public void CmdChangePlayerCount(Room room, int delta)
        // {
        //     if (room == null) return;
        //     
        //     room.PlayerCountServer += delta;
        //     CmdUpdatePlayerCount(room, room.PlayerCountServer);
        // }
        //
        // [Command(requiresAuthority = false)]
        // public void CmdUpdatePlayerCount(Room room, int count)
        // {
        //     for (int i = 0; i < map.Rooms.GetLength(0); i++)
        //     {
        //         for (int j = 0; j < map.Rooms.GetLength(1); j++)
        //         {
        //             if (map.Rooms[i, j] == room)
        //             {
        //                 RpcUpdatePlayerCount(new Vector2Int(i, j), count);
        //             }   
        //         }
        //     }
        // }
        //
        // [ClientRpc]
        // private void RpcUpdatePlayerCount(Vector2Int pos, int count)
        // {
        //     MinimapManager.Instance.UpdatePlayerCount(pos, count);
        // }

        private void MapHook(Map oldMap, Map newMap)
        {
            OnRoomsChanged?.Invoke();
            
            if (GameManager.GamePhase == GamePhase.Dungeon)
            {
                TeleportPlayerToStart();
            
                FindObjectOfType<SmoothCameraFollow>().FixateOnPlayer();
                
                TransitionController.Instance.FadeIn();
            }
        }

        private bool InStartingRoom => _currentActiveRoomIndex == map.StartingRoomPos;
        public bool CanSwitchPhases => InStartingRoom;
        public event Action OnRoomsChanged;
        public static Action OnActiveRoomChanged;
        public static Action<Player, Room> OnActiveRoomChangedGlobal;
        
        private void Awake()
        {
            //Singleton code
            Instance = this;
            //
        }

        private void Start()
        {
            _smoothCameraFollow = FindObjectOfType<SmoothCameraFollow>(); //Initialize the camera follow variables

            OnRoomsChanged += MinimapManager.Instance.Reset;
            OnRoomsChanged += MinimapManager.Instance.UpdateMinimap;
            OnRoomsChanged += DoDoorClosing;
            OnRoomsChanged?.Invoke();

            OnActiveRoomChanged += () => CmdBroadcastRoomChange(Player.LocalInstance, currentActiveRoom); 

            if (isServer) StartCoroutine(GenerateWhenReady());
        }

        [Command(requiresAuthority = false)]
        private void CmdBroadcastRoomChange(Player sender, Room room)
        {
            RpcBroadcastRoomChange(sender, room);
        }

        [ClientRpc]
        private void RpcBroadcastRoomChange(Player sender, Room room)
        {
            OnActiveRoomChangedGlobal.Invoke(sender, room);
        }

        [Command(requiresAuthority = false)]
        public void CmdUncoverRoom(Vector2Int pos)
        {
            RpcUncoverRoom(pos);
        }

        [ClientRpc]
        private void RpcUncoverRoom(Vector2Int pos)
        {
            MinimapManager.Instance.UncoverRoom(pos);
        }

        private IEnumerator GenerateWhenReady()
        {
            yield return new WaitUntil(()=>CustomNetworkManager.ReadyToStart);
            GenerateRooms();
        }

        [ServerCallback]
        public void GenerateRooms()
        {
            CmdSetMap(GameManager.DungeonFloor != 1 && GameManager.DungeonFloor % 10 == 0 ? roomGenerator.GenerateBossMap(GameManager.DungeonFloor) : roomGenerator.Generate(GameManager.DungeonFloor)); //Randomly generate rooms
        }

        public void ShiftActiveRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            Player.LocalInstance.playerMovement.DoMovement = false;
            StartCoroutine(MovePlayerToNextRoom(direction));
        }

        private IEnumerator MovePlayerToNextRoom(Direction direction) //Moves a player to the next room in a direction.
        {
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
            
            TransitionController.Instance.FadeInOut(); //Transition to make it less jarring.
            yield return
                new WaitUntil(() =>
                    TransitionController.Instance.OutTransitionDone); //Wait until the transition is over

            if (room != null)
            {
                SetActiveRoom(_currentActiveRoomIndex.x + dx,
                    _currentActiveRoomIndex.y + dy); //Set the active room to the new room

                //Move the player into the newly activated room
                var playerTransform = Player.LocalInstance.transform;
                playerTransform.position = room.GetSpawnpoint(direction);
                Player.LocalInstance.playerMovement.DoMovement = true;
                //
            }
            else
            {
                if(currentActiveRoom != null) Player.LocalInstance.transform.position = currentActiveRoom.transform.position;
                Player.LocalInstance.playerMovement.DoMovement = true;
            }
        }

        public void SetActiveRoom(int x, int y)
        {
            //if(currentActiveRoom != null) CmdChangePlayerCount(currentActiveRoom, -1);
            
            var newRoom = map.Rooms[x, y]; //Get the new room from the array
            if (newRoom == null)
            {
                Debug.LogWarning("New room is null!");
                return; //If it's null, do nothing
            }

            currentActiveRoom = newRoom; //Update currentActiveRoom variable
            currentActiveRoom.gameObject.SetActive(true); //Activate the new room
            
            //if(currentActiveRoom != null) CmdChangePlayerCount(currentActiveRoom, 1);
            
            CmdUncoverRoom(new Vector2Int(x, y)); //Uncover the room on the minimap
            _smoothCameraFollow.SetCameraBounds(currentActiveRoom.RoomRect); //Update the camera bounds
            _currentActiveRoomIndex = new Vector2Int(x, y); //Update currentActiveRoomIndex variable

            Player.LocalInstance.CmdSetParentIdentity(newRoom.netIdentity);
            
            OnActiveRoomChanged?.Invoke();
        }

        //Performs all actions necessary to disable the room phase.
        public void DeactivateRoomPhase()
        {
            _currentActiveRoomIndex = Vector2Int.zero;
            if (_smoothCameraFollow != null) _smoothCameraFollow.SetCameraBounds(Rect.zero);

            Player.LocalInstance.CmdSetParentIdentity(null);

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

        [Server]
        public void ResetRooms()
        {
            foreach (var r in map.Rooms) //Destroy all rooms and their child objects...
                if (r != null)
                {
                    r.enabled = false;

                    r.roomChildObjects = r.roomChildObjects.Where(go => go != null).ToList();
                    foreach (var go in r.roomChildObjects.ToList().Where(go => go.GetComponent<Player>() == null))
                    {
                       var e = go.GetComponent<Enemy>();

                        if (e != null)
                        {
                            WaveManager.Instance.MoveEnemyToWave(e);
                            continue;
                        }
                        
                        if (go != null)
                        {
                            var networkIdentity = go.GetComponent<NetworkIdentity>();
                            if(networkIdentity == null) Destroy(go);
                            else NetworkServer.Destroy(go);
                        }
                    }
                    
                    NetworkServer.Destroy(r.gameObject);
                }

            DeactivateRoomPhase();

            MinimapManager.Instance.Reset();
            roomGenerator.Reset();
        }
        
        [ClientRpc]
        public void RpcResetRooms()
        {
            foreach (var r in map.Rooms) //Destroy all rooms and their child objects...
                if (r != null)
                {
                    r.roomChildObjects = r.roomChildObjects.Where(go => go != null).ToList();
                    foreach (var go in r.roomChildObjects.Where(go => go.GetComponent<NetworkIdentity>() == null))
                    {
                        if (go.GetComponent<MoneyObject>() != null)
                        {
                            go.SetActive(false);
                            continue;
                        }
                        
                        Destroy(go);
                    }
                }
        }

        public void TeleportPlayerToStart()
        {
            Player.LocalInstance.transform.position = map.StartingRoom.transform.position;
            SetActiveRoom(map.StartingRoomPos.x, map.StartingRoomPos.y);
        }
    }
}