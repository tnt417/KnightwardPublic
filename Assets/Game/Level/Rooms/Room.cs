using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Rooms
{
    public static class ReadWriteRooms
    {
        public static void WriteRoomArray(this NetworkWriter writer, Room[,] value)
        {
            var isNull = value == null;
            writer.WriteBool(isNull);
            
            if (isNull) return;

            var dimension0 = value.GetLength(0);
            var dimension1 = value.GetLength(1);

            writer.WriteInt(dimension0);
            writer.WriteInt(dimension1); //write the number of dimensions of the 2nd dimension

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    var netId = value[i, j]?.netIdentity;
                    writer.WriteNetworkIdentity(netId);
                }
            }
        }
        public static Room[,] ReadRoomArray(this NetworkReader reader)
        {
            var isNull = reader.ReadBool();

            if (isNull) return null;
            
            var dimension0 = reader.ReadInt();
            var dimension1 = reader.ReadInt();

            var rooms = new Room[dimension0, dimension1];

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    var netId = reader.ReadNetworkIdentity();
                    if (netId == null) continue;
                    rooms[i, j] = netId.GetComponent<Room>();
                }
            }

            return rooms;
        }

        public static void WriteMap(this NetworkWriter writer, Map value)
        {
            writer.WriteRoomArray(value.Rooms);
            writer.WriteVector2Int(value.StartingRoomPos);
        }
        public static Map ReadMap(this NetworkReader reader, Map value)
        {
            var rooms = reader.ReadRoomArray();
            var startingRoomPos = reader.ReadVector2Int();
            return new Map(rooms, startingRoomPos);
        }
    }

    public class Room : NetworkBehaviour
    {
        //Editor variables
        [SerializeField] private RoomDoor[] roomDoors;
        [SerializeField] private GameObject spawnPrefabOnClear;
        [SerializeField] private Transform entryPointUp;
        [SerializeField] private Transform entryPointDown;
        [SerializeField] private Transform entryPointLeft;
        [SerializeField] private Transform entryPointRight;

        public Sprite minimapIcon;
        //
        private List<Direction> _openDirections;
        public Rect RoomRect => FindRoomRect();
        private bool _clearPrefabSpawned;

        private readonly SyncDictionary<Direction, bool> _openDoorsDictionary = new ();
        
        //The child GameObjects of this room, as dictated by IHideable
        public List<GameObject> roomChildObjects = new ();

        private void OnOpenDoorsDictionaryChange(SyncDictionary<Direction, bool>.Operation op, Direction key, bool open)
        {
            roomDoors.FirstOrDefault(rd => rd.direction == key)?.SetOpen(open);
        }

        [Command(requiresAuthority = false)]
        private void CmdSetDoorOpen(Direction direction, bool open)
        {
            _openDoorsDictionary[direction] = open;
        }

        [ClientCallback]
        private void CheckRoomVisibility(NetworkIdentity newRoom)
        {
            if (Player.LocalInstance == null || this == null) return;
            SetVisibility(newRoom == netIdentity);
        }

        private void SetVisibility(bool visible)
        {
            foreach (var rend in GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (var img in GetComponentsInChildren<Image>())
                img.enabled = visible;
            foreach (var l2d in GetComponentsInChildren<Light2DBase>())
                l2d.enabled = visible;
            foreach (var coll in GetComponentsInChildren<Collider2D>())
                coll.enabled = visible;
            foreach (var roomDoor in GetComponentsInChildren<RoomDoor>())
                roomDoor.SetHostVisibility(visible);
        }

        private void Awake()
        {
            Player.LocalInstance.OnParentIdentityChange += CheckRoomVisibility;
            _openDoorsDictionary.Callback += OnOpenDoorsDictionaryChange;
        }

        private void Start()
        {
            GameManager.OnEnemyAdd += OnEntityChange;
            GameManager.OnEnemyRemove += OnEntityChange;
            CheckShouldLockDoors();
        }

        private void OnDestroy()
        {
            GameManager.OnEnemyAdd -= OnEntityChange;
            GameManager.OnEnemyRemove -= OnEntityChange;
        }

        //Returns a rect, representing the room's position and shape in the world based on its tilemaps
        private Rect FindRoomRect()
        {
            float xMin = 0;
            float yMin = 0;
            float xMax = 0;
            float yMax = 0;

            foreach (var tm in GetComponentsInChildren<Tilemap>())
            {
                tm.CompressBounds();
                if (xMin > tm.cellBounds.xMin) xMin = tm.cellBounds.xMin;
                if (yMin > tm.cellBounds.yMin) yMin = tm.cellBounds.yMin;
                if (xMax < tm.cellBounds.xMax) xMax = tm.cellBounds.xMax;
                if (yMax < tm.cellBounds.yMax) yMax = tm.cellBounds.yMax;
            }

            var pos = transform.position;

            return new Rect(pos.x + xMin, pos.y + yMin, xMax - xMin, yMax - yMin);
        }

        public Vector2 GetSpawnpoint(Direction fromDirection)
        {
            return fromDirection switch
            {
                Direction.Up => entryPointDown.position,
                Direction.Down => entryPointUp.position,
                Direction.Right => entryPointLeft.position,
                Direction.Left => entryPointRight.position,
                _ => transform.position
            };
        }

        [ServerCallback]
        public void SetOpenDirections(List<Direction> directions) //Opens doors based on the provided list and updates this class' open directions list.
        {
            if (this == null) return;
            _openDirections = directions;
            foreach (var d in directions)
            {
                CmdSetDoorOpen(d, true);
            }
        }

        public List<Direction> GetDoorDirections()
        {
            return roomDoors.Select(rd => rd.direction).ToList();
        }

        private void CheckShouldLockDoors()
        {
            if (this == null) return;

            var enemySpawner = GetComponentInChildren<EnemySpawner>();

            var shouldLock = GameManager.EntitiesReadonly.Any(entity =>
                                 entity is Enemy {IsAlive: true} && entity.CurrentParentIdentity == netIdentity)
                             /*|| enemySpawner != null && enemySpawner.CurrentlySpawning*/; //Check if there are any alive enemies in our room or if our spawner is spawning.

            if (shouldLock)
            {
                LockAllDoors(); //Lock doors while enemies are alive in the room.
            }
            else
            {
                OnClear();
            }
        }

        public void OnEnemySpawn()
        {
            LockAllDoors();
        }

        private void OnEntityChange(GameEntity entity)
        {
            if (this == null) return;
            CheckShouldLockDoors();
        }

        [ServerCallback]
        private void OnClear()
        {
            if (!_clearPrefabSpawned && spawnPrefabOnClear != null)
            {
                var myTransform = transform;
                Instantiate(spawnPrefabOnClear, myTransform.position, quaternion.identity,
                    myTransform); //Instantiate the on clear prefab in the center of the room
                _clearPrefabSpawned = true; //TODO: won't work on multiplayer
            }

            OpenAllDoors(); //Otherwise, open/close the doors as normal.
        }

        [ServerCallback]
        private void OpenAllDoors()
        {
            if (this == null || _openDirections == null) return;
            foreach (var rd in roomDoors)
            {
                if (_openDirections.Contains(rd.direction)) CmdSetDoorOpen(rd.direction, true);
                else CmdSetDoorOpen(rd.direction, false);
            }
        }

        [ServerCallback]
        private void LockAllDoors() //Closes all doors
        {
            if (this == null) return;
            foreach (var rd in roomDoors)
            {
                CmdSetDoorOpen(rd.direction, false);
            }
        }
    }
}