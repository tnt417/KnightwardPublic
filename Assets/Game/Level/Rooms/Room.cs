using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.Level.Rooms
{
    public static class ReadWriteRooms
    {
        public static void WriteRoomArray(this NetworkWriter writer, Room[,] value)
        {
            var dimension0 = value.GetLength(0);
            var dimension1 = value.GetLength(1);

            writer.WriteInt(dimension0);
            writer.WriteInt(dimension1); //write the number of dimensions of the 2nd dimension

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    writer.Write(value[i, j]?.netIdentity);
                }
            }
        }

        public static Room[,] ReadRoomArray(this NetworkReader reader)
        {
            var dimension0 = reader.ReadInt();
            var dimension1 = reader.ReadInt();

            var rooms = new Room[dimension0, dimension1];

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    var netId = reader.Read<NetworkIdentity>();
                    if (netId == null) continue;
                    rooms[i, j] = netId.GetComponent<Room>();
                }
            }

            return rooms;
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

        private Vector2Int _roomIndex;
        private List<Direction> _openDirections;
        public Rect RoomRect => FindRoomRect();
        private bool _clearPrefabSpawned;

        private void Start()
        {
            GameManager.OnEnemyAdd += OnEntityChange;
            GameManager.OnEnemyRemove += OnEntityChange;
            CheckShouldLockDoors();
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

        public void
            SetOpenDirections(
                List<Direction> directions) //Opens doors based on the provided list and updates this class' open directions list.
        {
            _openDirections = directions;
            foreach (var rd in
                from d in directions
                from rd in roomDoors
                where rd.direction == d
                select rd) //Not sure what Ryder did for me here
            {
                rd.Open();
            }
        }

        public List<Direction> GetDoorDirections()
        {
            return roomDoors.Select(rd => rd.direction).ToList();
        }

        private void CheckShouldLockDoors()
        {
            var enemySpawner = GetComponentInChildren<EnemySpawner>();

            var shouldLock = GameManager.EntitiesReadonly.Any(entity =>
                                 entity is Enemy {IsAlive: true} && entity.currentParentIdentity == netIdentity)
                             || enemySpawner != null && enemySpawner.CurrentlySpawning; //Check if there are any alive enemies in our room or if our spawner is spawning.

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

        public void OnEntityChange(GameEntity entity)
        {
            CheckShouldLockDoors();
        }

        private void OnClear()
        {
            if (!_clearPrefabSpawned && spawnPrefabOnClear != null)
            {
                var myTransform = transform;
                Instantiate(spawnPrefabOnClear, myTransform.position, quaternion.identity,
                    myTransform); //Instantiate the on clear prefab in the center of the room
                _clearPrefabSpawned = true;
            }

            OpenAllDoors(); //Otherwise, open/close the doors as normal.
        }

        private void OpenAllDoors()
        {
            if (_openDirections == null) return;
            foreach (var rd in roomDoors)
            {
                if (_openDirections.Contains(rd.direction)) rd.Open();
                else rd.Close();
            }
        }

        private void LockAllDoors() //Closes all doors
        {
            foreach (var rd in roomDoors)
            {
                rd.Close();
            }
        }

        public void SetRoomIndex(Vector2Int v2) //Called with event. Communicates where this room is on the grid.
        {
            _roomIndex = v2;
        }
    }
}