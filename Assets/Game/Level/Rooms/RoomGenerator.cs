using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms
{
    [Serializable]
    public struct SerializableMap
    {
        public SerializableMap(uint[,] rooms, Vector2Int startingRoomPos)
        {
            Rooms = rooms;
            StartingRoomPos = startingRoomPos;
        }

        public readonly uint[,] Rooms;
        public readonly Vector2Int StartingRoomPos;

        public static SerializableMap FromMap(Map map)
        {
            if (map.Rooms == null) return new SerializableMap(null, Vector2Int.zero);

            var width = map.Rooms.GetLength(0);
            var height = map.Rooms.GetLength(1);

            var ids = new uint[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var room = map.Rooms[x, y];

                    if (room == null) continue;

                    ids[x, y] = map.Rooms[x, y].netId;
                }
            }

            return new SerializableMap(ids, map.StartingRoomPos);
        }
    }

    public readonly struct Map
    {
        public static Map FromSerializable(SerializableMap serializableMap)
        {
            if (serializableMap.Rooms == null) return new Map(null, Vector2Int.zero);

            var width = serializableMap.Rooms.GetLength(0);
            var height = serializableMap.Rooms.GetLength(1);

            var rooms = new Room[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    rooms[x, y] = RoomManager.Instance.GetRoomFromID(serializableMap.Rooms[x, y]);
                }
            }

            return new Map(rooms, serializableMap.StartingRoomPos);
        }

        public Map(Room[,] rooms, Vector2Int startingRoomPos)
        {
            Rooms = rooms;
            StartingRoomPos = startingRoomPos;
        }

        public readonly Room[,] Rooms;
        public readonly Vector2Int StartingRoomPos;
        public Room StartingRoom => Rooms[StartingRoomPos.x, StartingRoomPos.y];

        public Vector2Int GetRoomPos(Room room)
        {
            for (var i = 0; i < Rooms.GetLength(0); i++)
            {
                for (var j = 0; j < Rooms.GetLength(1); j++)
                {
                    if (Rooms[i, j] == room) return new Vector2Int(i, j);
                }
            }

            return new Vector2Int(0, 0);
        }
    }

    public enum RoomGenerateTier
    {
        Common,
        Uncommon,
        Special,
        Guaranteed,
        Boss
    }

    [Serializable]
    public struct RoomEntry
    {
        public RoomGenerateTier tier;
        public GameObject roomPrefab;
    }

    public class RoomGenerator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public int mapRadius;
        [SerializeField] public Vector2 roomOffset;
        [SerializeField] public MapGenConfig config;

        [SerializeField] private RoomManager roomManager;
        //

        private Vector2Int _startingRoomPos;
        public int MapSize => mapRadius * 2 + 1;
        private bool _generated = false;
        private int _seed;

        private void Awake()
        {
            _startingRoomPos = new Vector2Int(mapRadius, mapRadius);
            _seed = Random.Range(0, 100000); //Get a random seed to generate the room based on.

            foreach (var prefab in config.mapZones.SelectMany(mz => mz.roomEntries.Select(r => r.roomPrefab)))
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        public void Reset()
        {
            _generated = false;
        }

        [Server]
        public Map Generate(int floor) //Returns an array of randomly generated rooms
        {
            if (!GameManager.Instance.isServer) return default;

            if (_generated) return roomManager.Map;

            var theme = config.mapZones.Where(z => floor % config.loopPoint >= z.startFloor)
                .OrderByDescending(z => z.startFloor).FirstOrDefault();

            var guaranteedGeneratePrefabs = theme.roomEntries.Where(r => r.tier == RoomGenerateTier.Guaranteed)
                .Select(r => r.roomPrefab).ToArray();

            var roomCount = Random.Range(theme.roomAmountRange.x, theme.roomAmountRange.y);

            var generateShape = GetMapShape(roomCount);

            var rooms = new Room[MapSize, MapSize];
            var prefabs = new GameObject[MapSize, MapSize];

            var remainingGuaranteed = guaranteedGeneratePrefabs.Length;
            var remainingSpecial = theme.specialAmount;
            var remainingUncommon = theme.uncommonAmount;
            var remainingCommon = roomCount - remainingGuaranteed - remainingSpecial - remainingUncommon;

            var entryList = theme.roomEntries.ToList();
            var chosenPrefabs = new List<GameObject>();

            var genTimeout = Time.time + 2f;

            while (remainingGuaranteed + remainingSpecial + remainingUncommon + remainingCommon > 0 && Time.time < genTimeout)
            {
                var tier = remainingGuaranteed == 0
                    ? remainingSpecial == 0 ? remainingUncommon == 0 ? RoomGenerateTier.Common :
                    RoomGenerateTier.Uncommon : RoomGenerateTier.Special
                    : RoomGenerateTier.Guaranteed;

                if (tier == RoomGenerateTier.Common)
                {
                    if (remainingCommon == 0) continue;
                    remainingCommon--;
                }

                if (tier == RoomGenerateTier.Uncommon)
                {
                    if (remainingUncommon == 0) continue;
                    remainingUncommon--;
                }

                if (tier == RoomGenerateTier.Special)
                {
                    if (remainingSpecial == 0) continue;
                    remainingSpecial--;
                }

                if (tier == RoomGenerateTier.Guaranteed)
                {
                    if (remainingGuaranteed == 0) continue;
                    remainingGuaranteed--;
                }

                var chosenEntry = GameTools.SelectRandom(entryList.Where(r => r.tier == tier));

                entryList.Remove(chosenEntry);

                chosenPrefabs.Add(chosenEntry.roomPrefab); //Otherwise, generate a random prefab from the list
            }

            if (Time.time > genTimeout)
            {
                Debug.LogWarning("Could not generate! Try changing the generation settings.");
                return default;
            }

            for (var i = 0; i < MapSize; i++)
            {
                for (var j = 0; j < MapSize; j++)
                {
                    if (generateShape[i, j] == 0) continue; //If the shape says not to generate, move on

                    var prefab = GameTools.SelectRandom(chosenPrefabs); //Randomly scramble the prefabs across the map

                    if (prefab.CompareTag("StartRoom")) _startingRoomPos = new Vector2Int(i, j);

                    prefabs[i, j] = prefab;

                    chosenPrefabs.Remove(prefab);
                }
            }

            for (var i = 0; i < MapSize; i++) //Generate every room
            for (var j = 0; j < MapSize; j++)
            {
                rooms[i, j] = GenerateRoom(new Vector2Int(i, j), prefabs[i, j]);
            }

            _generated = true;
            return new Map(rooms, _startingRoomPos);
        }

        [Server]
        public Map GenerateBossMap(int floor)
        {
            var theme = config.mapZones.Where(z => (floor - 1) % config.loopPoint + 1 >= z.startFloor)
                .OrderByDescending(z => z.startFloor).FirstOrDefault();

            var rooms = new Room[MapSize, MapSize];
            rooms[mapRadius, mapRadius] = GenerateRoom(new Vector2Int(mapRadius, mapRadius),
                GameTools.SelectRandom(theme.roomEntries.Where(re => re.tier == RoomGenerateTier.Boss)).roomPrefab);

            return new Map(rooms, new Vector2Int(mapRadius, mapRadius));
        }

        private Room GenerateRoom(Vector2Int index, GameObject prefab)
        {
            if (prefab == null) return null;

            //Instantiate a room prefab, set its index, deactivate it, and add it to the array.
            var go = Instantiate(prefab,
                new Vector2(roomOffset.x * (index.x - mapRadius), roomOffset.y * (index.y - mapRadius)),
                Quaternion.identity);

            NetworkServer.Spawn(go);

            return go.GetComponent<Room>();
            //
        }

        //based on this website: https://www.boristhebrave.com/2020/09/12/dungeon-generation-in-binding-of-isaac/
        private int[,] GetMapShape(float roomCount)
        {
            var roomAllowance = roomCount;

            var mapShape = new int[MapSize + 2, MapSize + 2];

            mapShape[mapRadius, mapRadius] = 1; //Start with the middle room
            roomAllowance--;

            while (roomAllowance > 0)
            {
                //Look every room on the map
                for (var i = 0; i < MapSize; i++)
                {
                    for (var j = 0; j < MapSize; j++)
                    {
                        if (mapShape[i, j] != 1) continue;
                        //Keep going if the room exists

                        var neighborUp = mapShape[i, j + 1];
                        var neighborDown = mapShape[i, j - 1];
                        var neighborLeft = mapShape[i - 1, j];
                        var neighborRight = mapShape[i + 1, j];

                        //Check if a room should be generated up
                        if (neighborUp != 1)
                        {
                            var neighborAdjacentRoomCount = 0;
                            if (mapShape[i + 1, j + 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i - 1, j + 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j + 2] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j] == 1) neighborAdjacentRoomCount++;

                            if (neighborAdjacentRoomCount <= 1)
                            {
                                if (Random.Range(0, 100) < 50) //50% chance to generate
                                {
                                    mapShape[i, j + 1] = 1;
                                    roomAllowance--;
                                    if (roomAllowance <= 0) return mapShape;
                                }
                            }
                        }

                        //Check if a room should be generated down
                        if (neighborDown != 1)
                        {
                            var neighborAdjacentRoomCount = 0;
                            if (mapShape[i + 1, j - 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i - 1, j - 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j - 2] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j] == 1) neighborAdjacentRoomCount++;

                            if (neighborAdjacentRoomCount <= 1)
                            {
                                if (Random.Range(0, 100) < 50) //50% chance to generate
                                {
                                    mapShape[i, j - 1] = 1;
                                    roomAllowance--;
                                    if (roomAllowance <= 0) return mapShape;
                                }
                            }
                        }

                        //Check if a room should be generated left
                        if (neighborLeft != 1)
                        {
                            var neighborAdjacentRoomCount = 0;
                            if (mapShape[i - 1, j + 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i - 1, j - 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i - 2, j] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j] == 1) neighborAdjacentRoomCount++;

                            if (neighborAdjacentRoomCount <= 1)
                            {
                                if (Random.Range(0, 100) < 50) //50% chance to generate
                                {
                                    mapShape[i - 1, j] = 1;
                                    roomAllowance--;
                                    if (roomAllowance <= 0) return mapShape;
                                }
                            }
                        }

                        //Check if a room should be generated right
                        if (neighborRight != 1)
                        {
                            var neighborAdjacentRoomCount = 0;
                            if (mapShape[i + 1, j + 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i + 1, j - 1] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i + 2, j] == 1) neighborAdjacentRoomCount++;
                            if (mapShape[i, j] == 1) neighborAdjacentRoomCount++;

                            if (neighborAdjacentRoomCount <= 1)
                            {
                                if (Random.Range(0, 100) < 50) //50% chance to generate
                                {
                                    mapShape[i + 1, j] = 1;
                                    roomAllowance--;
                                    if (roomAllowance <= 0) return mapShape;
                                }
                            }
                        }
                    }
                }
            }

            return mapShape;
        }
    }
}