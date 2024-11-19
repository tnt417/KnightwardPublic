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
using UnityEngine.Serialization;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager Instance; //Singleton instance

        public Room currentActiveRoom;

        public readonly Dictionary<uint, Room> RoomIdentities = new();

        //Editor variables
        [SerializeField] public RoomGenerator roomGenerator;

        [SerializeField] private GameObject minimapObject;
        //

        private SmoothCameraFollow _smoothCameraFollow;

        //Room-related variables
        public int MapSize => roomGenerator.MapSize; //The shared width/height of the map
        public Vector2Int currentActiveRoomIndex;

        [SyncVar(hook = nameof(MapHook))] private SerializableMap _serializableMap;
        public Map Map => Map.FromSerializable(_serializableMap);

        [Command(requiresAuthority = false)]
        private void CmdSetMap(SerializableMap newMap)
        {
            _serializableMap = new SerializableMap(newMap.Rooms, newMap.StartingRoomPos, newMap.RoomConnections);
        }

        private void Update()
        {
            minimapObject.SetActive(GameManager.GamePhase == GamePhase.Dungeon);
            if (Player.LocalInstance != null && currentActiveRoom != null)
                MinimapManager.Instance.SetPlayerPosition(currentActiveRoom.GetLocalPlayerPositionNormalized());
        }

        public Room GetRoomFromID(NetworkIdentity networkIdentity) => GetRoomFromID(networkIdentity.netId);

        public Room GetRoomFromID(uint roomId)
        {
            return RoomIdentities.ContainsKey(roomId) ? RoomIdentities[roomId] : null;
        }

        private void MapHook(SerializableMap oldMap, SerializableMap newMap)
        {
            MapHookTask().Forget();
            
            OnRoomsChanged?.Invoke();
            
            if (GameManager.GamePhase == GamePhase.Dungeon)
            {
                TeleportPlayerToStart();
            
                FindObjectOfType<SmoothCameraFollow>().FixateOnPlayer();
                
                TransitionController.Instance.FadeIn();
            }
        }

        private async UniTask MapHookTask()
        {
            await UniTask.WaitUntil(() => Map.StartingRoom != null && Map.StartingRoom.netId != 0);
            await UniTask.Yield();
            
            OnRoomsChanged?.Invoke();
            
            if (GameManager.GamePhase == GamePhase.Dungeon)
            {
                TeleportPlayerToStart();
            
                FindObjectOfType<SmoothCameraFollow>().FixateOnPlayer();
                
                TransitionController.Instance.FadeIn();
            }
        }

        private bool InStartingRoom => currentActiveRoomIndex == Map.StartingRoomPos;
        public bool CanSwitchPhases => InStartingRoom;
        public Action OnRoomsChanged;
        public static Action OnActiveRoomChanged;
        public Action<Player, Room> OnActiveRoomChangedGlobal;
        public bool stopRoomChange = false;
        
        private void Awake()
        {
            //Singleton code
            Instance = this;
            //
        }

        public override void OnStartServer()
        {
            GenerateTask().Forget();
        }

        public override void OnStartClient()
        {
            _smoothCameraFollow = FindObjectOfType<SmoothCameraFollow>(); //Initialize the camera follow variables

            OnRoomsChanged += MinimapManager.Instance.Reset;
            OnRoomsChanged += MinimapManager.Instance.UpdateMinimap;
            OnRoomsChanged += DoDoorClosing;
            OnRoomsChanged?.Invoke();

            OnActiveRoomChanged += () => CmdBroadcastRoomChange(Player.LocalInstance, currentActiveRoom);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            OnActiveRoomChanged -= () => CmdBroadcastRoomChange(Player.LocalInstance, currentActiveRoom);
        }

        [Command(requiresAuthority = false)]
        private void CmdBroadcastRoomChange(Player sender, Room room)
        {
            RpcBroadcastRoomChange(sender, room);
        }

        [ClientRpc]
        private void RpcBroadcastRoomChange(Player sender, Room room)
        {
            OnActiveRoomChangedGlobal?.Invoke(sender, room);
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

        [Server]
        public async UniTask GenerateTask()
        {
            await UniTask.Yield();
            
            var newMap = roomGenerator.Generate(GameManager.DungeonFloor);

            await UniTask.WaitUntil(() =>
            {
                foreach (var r in newMap.Rooms)
                {
                    if(r == null) continue;
                    if (GetRoomFromID(r.netId) == null) return false;
                }

                return true;
            });

            Debug.Log("Generated!");

            CmdSetMap(SerializableMap.FromMap(newMap));
        }

        private bool _inRoomTransition = false;
        
        public void ShiftActiveRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            if (stopRoomChange) return;
            if (_inRoomTransition) return;
            _inRoomTransition = true;
            Player.LocalInstance.playerMovement.BetweenRoomMove(direction);
            StartCoroutine(MovePlayerToNextRoom(direction));
        }

        private IEnumerator MovePlayerToNextRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            Debug.Log("Moving player to the next room!");
        
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
            
            var room = Map.Rooms[currentActiveRoomIndex.x + dx, currentActiveRoomIndex.y + dy];
            
            TransitionController.Instance.FadeInOut(); //Transition to make it less jarring.
            yield return
                new WaitUntil(() =>
                    TransitionController.Instance.OutTransitionDone); //Wait until the transition is over

            if (room != null)
            {
                SetActiveRoom(currentActiveRoomIndex.x + dx,
                    currentActiveRoomIndex.y + dy); //Set the active room to the new room

                var directionVector = new Vector2(dx, dy);
                
                //Move the player into the newly activated room
                var playerTransform = Player.LocalInstance.transform;
                playerTransform.position = room.GetSpawnpoint(direction) - directionVector * 2;
                Player.LocalInstance.playerMovement.DoMovement = true;
                //
            }
            else
            {
                if(currentActiveRoom != null) Player.LocalInstance.transform.position = currentActiveRoom.transform.position;
                Player.LocalInstance.playerMovement.DoMovement = true;
            }
            
            _inRoomTransition = false;
        }

        public void SetActiveRoom(int x, int y)
        {
            //if(currentActiveRoom != null) CmdChangePlayerCount(currentActiveRoom, -1);
            
            var newRoom = Map.Rooms[x, y]; //Get the new room from the array
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
            currentActiveRoomIndex = new Vector2Int(x, y); //Update currentActiveRoomIndex variable

            Player.LocalInstance.CmdSetParentIdentity(newRoom.netIdentity);
            
            OnActiveRoomChanged?.Invoke();
        }

        //Performs all actions necessary to disable the room phase.
        public void DeactivateRoomPhase()
        {
            currentActiveRoomIndex = Vector2Int.zero;
            if (_smoothCameraFollow != null) _smoothCameraFollow.SetCameraBounds(Rect.zero);

            Player.LocalInstance.CmdSetParentIdentity(null);

            currentActiveRoom = null;
        }

        private void DoDoorClosing()
        {
            if (Map.Rooms == null)
            {
                Debug.LogWarning("Room array is null!");
                return;
            }

            for (var i = 0; i < roomGenerator.MapSize; i++) //Loop through all the rooms
            {
                for (var j = 0; j < roomGenerator.MapSize; j++)
                {
                    if (Map.Rooms[i, j] == null) continue; //If there's no room, move on

                    //Set open directions based on room adjacency
                    var openDirections = new List<Direction>();
                    if (Map.RoomConnections.Any(rc => rc.HasRoom(i,j) && rc.HasRoom(i-1, j)) && CheckIfRoomExistsAndHasDoor(i - 1, j, Direction.Right)) openDirections.Add(Direction.Left);
                    if (Map.RoomConnections.Any(rc => rc.HasRoom(i,j) && rc.HasRoom(i+1, j)) && CheckIfRoomExistsAndHasDoor(i + 1, j, Direction.Left)) openDirections.Add(Direction.Right);
                    if (Map.RoomConnections.Any(rc => rc.HasRoom(i,j) && rc.HasRoom(i, j-1)) && CheckIfRoomExistsAndHasDoor(i, j - 1, Direction.Up)) openDirections.Add(Direction.Down);
                    if (Map.RoomConnections.Any(rc => rc.HasRoom(i,j) && rc.HasRoom(i, j+1)) && CheckIfRoomExistsAndHasDoor(i, j + 1, Direction.Down)) openDirections.Add(Direction.Up);
                    Map.Rooms[i, j].SetOpenDirections(openDirections);
                    MinimapManager.Instance.UpdateOpenDirections(new Vector2Int(i,j), openDirections);
                    //
                }
            }
        }

        private bool CheckIfRoomExistsAndHasDoor(int x, int y, Direction direction)
        {
            if (x < 0 || y < 0 || x > roomGenerator.MapSize - 1 || y > roomGenerator.MapSize - 1) return false;
            return Map.Rooms[x, y] != null && Map.Rooms[x, y].GetDoorDirections().Contains(direction);
        }

        [Server]
        public void ResetRooms()
        {
            Debug.Log("Resetting rooms!");
        
            foreach (var r in Map.Rooms)
            {
                //Destroy all rooms and their child objects...
                if (r == null) continue;
             
                Debug.Log(r.name);
                
                r.enabled = false;

                var childObjects = r.roomChildObjects.Where(go => go != null && go.GetComponent<Player>() == null);
                foreach (var go in childObjects.ToList())
                {
                    if (go == null) continue;

                    var money = MoneyObject.MoneyObjectPool.FirstOrDefault(kv => kv.Key == go).Key;
                    
                    if (money != null)
                    {
                        money.SetActive(false);
                        continue;
                    }
                    
                    var e = go.GetComponent<Enemy>();

                    if (e != null)
                    {
                        WaveManager.Instance.MoveEnemyToWave(e);
                        continue;
                    }

                    if (go != null)
                    {
                        var networkIdentity = go.GetComponent<NetworkIdentity>();
                        if (networkIdentity == null) Destroy(go);
                        else NetworkServer.Destroy(go);
                    }
                }

                NetworkServer.Destroy(r.gameObject);
            }

            DeactivateRoomPhase();

            MinimapManager.Instance.Reset();
            roomGenerator.Reset();
            
            RoomIdentities.Clear();
        }
        
        [ClientRpc]
        public void RpcResetRooms()
        {
            foreach (var r in Map.Rooms) //Destroy all rooms and their child objects...
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
            Player.LocalInstance.transform.position = Map.StartingRoom.transform.position;
            SetActiveRoom(Map.StartingRoomPos.x, Map.StartingRoomPos.y);
        }
    }
}