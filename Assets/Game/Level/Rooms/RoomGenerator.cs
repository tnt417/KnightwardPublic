using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using TonyDev.Game.Global;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms
{
    [Serializable]
    public struct RoomConnection
    {
        public RoomConnection(Vector2Int posA, Vector2Int posB)
        {
            a = posA;
            b = posB;
        }

        public bool HasRoom(int x, int y)
        {
            var v2i = new Vector2Int(x, y);
            return a == v2i || b == v2i;
        }

        public Vector2Int a;
        public Vector2Int b;
    }

    [Serializable]
    public struct SerializableMap
    {
        public SerializableMap(uint[,] rooms, Vector2Int startingRoomPos, RoomConnection[] roomConnections)
        {
            Rooms = rooms;
            StartingRoomPos = startingRoomPos;
            RoomConnections = roomConnections;
        }

        public readonly uint[,] Rooms;
        public readonly RoomConnection[] RoomConnections;
        public readonly Vector2Int StartingRoomPos;

        public static SerializableMap FromMap(Map map)
        {
            if (map.Rooms == null) return new SerializableMap(null, Vector2Int.zero, null);

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

            return new SerializableMap(ids, map.StartingRoomPos, map.RoomConnections);
        }
    }

    public readonly struct Map
    {
        public static Map FromSerializable(SerializableMap serializableMap)
        {
            if (serializableMap.Rooms == null) return new Map(null, Vector2Int.zero, null);

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

            return new Map(rooms, serializableMap.StartingRoomPos, serializableMap.RoomConnections);
        }

        public Map(Room[,] rooms, Vector2Int startingRoomPos, RoomConnection[] roomConnections)
        {
            Rooms = rooms;
            StartingRoomPos = startingRoomPos;
            RoomConnections = roomConnections;
        }

        public readonly Room[,] Rooms;
        public readonly Vector2Int StartingRoomPos;
        public readonly RoomConnection[] RoomConnections;
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
    public class RoomEntry
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

        // Static accessor to use in PhaseInformationUIController.cs
        public static MapGenConfig Config => RoomManager.Instance.roomGenerator.config;

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
        public Map Generate(int floor)
        {
            if (!GameManager.Instance.isServer) return default;

            if (_generated) return roomManager.Map;

            floor += config.floorAtLaunchOffset;

            var theme = config.mapZones.Where(z => floor % config.loopPoint >= z.startFloor)
                .OrderByDescending(z => z.startFloor).FirstOrDefault();

            var remainingEntries = theme.roomEntries.ToList();

            var rooms = new Room[MapSize, MapSize];

            var roomConnections = new List<RoomConnection>();

            var roomCount = Random.Range(theme.roomAmountRange.x, theme.roomAmountRange.y);

            var i = mapRadius;
            var j = mapRadius;

            var remainingGuaranteed = remainingEntries.Count(re => re.tier == RoomGenerateTier.Guaranteed);
            var remainingSpecial = theme.specialAmount;
            var remainingUncommon = theme.uncommonAmount;
            var remainingCommon = roomCount - remainingGuaranteed - remainingSpecial - remainingUncommon;

            var startPos = Vector2Int.zero;

            for (var r = 0; r < roomCount - remainingSpecial; r++)
            {
                var curPos = new Vector2Int(i, j);
                var validNeighbors = GetOpenNeighborPositions(curPos, rooms)
                    .Where(nb => GetOpenNeighborPositions(nb, rooms).Count == (r == 0 ? 4 : 3));

                var chosenNeighbor = GameTools.SelectRandom(validNeighbors);

                roomConnections.Add(new RoomConnection(curPos,
                    chosenNeighbor));

                i = chosenNeighbor.x;
                j = chosenNeighbor.y;

                if (r == 0)
                {
                    var entry = remainingEntries.FirstOrDefault(re => re.roomPrefab.CompareTag("StartRoom"));
                    rooms[i, j] = GenerateRoom(new Vector2Int(i, j), entry.roomPrefab);
                    remainingGuaranteed--;
                    remainingEntries.Remove(entry);
                    startPos = new Vector2Int(i, j);
                }
                else if (r == roomCount - remainingSpecial - 1)
                {
                    var entry = remainingEntries.FirstOrDefault(re =>
                        re.tier == RoomGenerateTier.Guaranteed && !re.roomPrefab.CompareTag("StartRoom"));
                    rooms[i, j] = GenerateRoom(new Vector2Int(i, j), entry.roomPrefab);
                    remainingGuaranteed--;
                    remainingEntries.Remove(entry);
                }
                // else if (remainingGuaranteed > 0)
                // {
                //     var entry =
                //         GameTools.SelectRandom(remainingEntries.Where(re => re.tier == RoomGenerateTier.Guaranteed));
                //     rooms[i,j] = GenerateRoom(new Vector2Int(i, j), entry.roomPrefab);
                //     remainingGuaranteed--;
                //     remainingEntries.Remove(entry);
                // }
                else
                {
                    var entry =
                        GameTools.SelectRandom(remainingEntries.Where(re => re.tier == RoomGenerateTier.Common));

                    rooms[i, j] = GenerateRoom(new Vector2Int(i, j), entry.roomPrefab);
                    remainingCommon--;
                    remainingEntries.Remove(entry);
                }
            }

            var positions = new List<Vector2Int>();

            for (var x = 0; x < MapSize; x++)
            {
                for (var y = 0; y < MapSize; y++)
                {
                    if (rooms[x, y] != null) positions.Add(new Vector2Int(x, y));
                }
            }

            for (var t = 0; t < remainingSpecial; t++)
            {
                var entry =
                    GameTools.SelectRandom(remainingEntries.Where(re => re.tier == RoomGenerateTier.Special));

                var trunk = GameTools.SelectRandom(positions.Where(pos =>
                    GetOpenNeighborPositions(pos, rooms).Count == 2));

                var validRoomPos = GameTools.SelectRandom(GetOpenNeighborPositions(
                    trunk,
                    rooms)); // A random room with 2 bordering rooms (not start or end room) and a random open pos next to it.

                roomConnections.Add(new RoomConnection(trunk, validRoomPos));

                rooms[validRoomPos.x, validRoomPos.y] = GenerateRoom(validRoomPos, entry.roomPrefab);
                remainingEntries.Remove(entry);
            }

            for (var t = 0; t < remainingUncommon; t++)
            {
                var entry =
                    GameTools.SelectRandom(remainingEntries.Where(re => re.tier == RoomGenerateTier.Uncommon));

                var trunk = GameTools.SelectRandom(positions.Where(pos =>
                    GetOpenNeighborPositions(pos, rooms).Count == 2));

                var validRoomPos = GameTools.SelectRandom(GetOpenNeighborPositions(
                    trunk,
                    rooms)); // A random room with 2 bordering rooms (not start or end room) and a random open pos next to it.

                roomConnections.Add(new RoomConnection(trunk, validRoomPos));
                
                rooms[validRoomPos.x, validRoomPos.y] = GenerateRoom(validRoomPos, entry.roomPrefab);
                remainingEntries.Remove(entry);
            }

            return new Map(rooms, startPos, roomConnections.ToArray());
        }

        /*
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

            var rooms = new Room[MapSize, MapSize];
            var prefabs = new GameObject[MapSize, MapSize];

            var remainingGuaranteed = guaranteedGeneratePrefabs.Length;
            var remainingSpecial = theme.specialAmount;
            var remainingUncommon = theme.uncommonAmount;
            var remainingCommon = roomCount - remainingGuaranteed - remainingSpecial - remainingUncommon;

            var generateShape = GetMapShape(roomCount, remainingSpecial + remainingUncommon, out var twigs,
                out var endPos);

            var entryList = theme.roomEntries.ToList();
            var chosenPrefabs = new List<GameObject>();

            var genTimeout = Time.time + 2f;

            for (var i = 0; i < twigs.Length; i++)
            {
                var tier = remainingSpecial == 0 ? RoomGenerateTier.Uncommon : RoomGenerateTier.Special;

                var entry = GameTools.SelectRandom(entryList.Where(r => r.tier == tier));

                if (tier == RoomGenerateTier.Uncommon)
                {
                    remainingUncommon--;
                }

                if (tier == RoomGenerateTier.Special)
                {
                    remainingSpecial--;
                }

                prefabs[twigs[i].x, twigs[i].y] = entry.roomPrefab;

                entryList.Remove(entry);
            }

            while (remainingGuaranteed + remainingSpecial + remainingUncommon + remainingCommon > 0 &&
                   Time.time < genTimeout)
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

                if (tier != RoomGenerateTier.Guaranteed)
                {
                    var chosenEntry = GameTools.SelectRandom(entryList.Where(r => r.tier == tier));
                    entryList.Remove(chosenEntry);
                    chosenPrefabs.Add(chosenEntry.roomPrefab); //Otherwise, generate a random prefab from the list
                }
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
                    if (generateShape[i, j] == 0 || twigs.Contains(new Vector2Int(i, j)))
                        continue; //If the shape says not to generate, move on

                    var prefab = i == mapRadius && j == mapRadius
                        ? entryList.First(e =>
                            e.tier == RoomGenerateTier.Guaranteed && e.roomPrefab.CompareTag("StartRoom")).roomPrefab
                        : GameTools.SelectRandom(chosenPrefabs); //Randomly scramble the prefabs across the map

                    if (i == endPos.x && j == endPos.y)
                    {
                        prefab = entryList.First(e =>
                            e.tier == RoomGenerateTier.Guaranteed && !e.roomPrefab.CompareTag("StartRoom")).roomPrefab;
                    }

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
*/
        
        [Server][Obsolete("Bosses are no longer implemented.")]
        public Map GenerateBossMap(int floor)
        {
            var theme = config.mapZones.Where(z => (floor - 1) % config.loopPoint + 1 >= z.startFloor)
                .OrderByDescending(z => z.startFloor).FirstOrDefault();

            var rooms = new Room[MapSize, MapSize];
            rooms[mapRadius, mapRadius] = GenerateRoom(new Vector2Int(mapRadius, mapRadius),
                GameTools.SelectRandom(theme.roomEntries.Where(re => re.tier == RoomGenerateTier.Boss)).roomPrefab);

            return new Map(rooms, new Vector2Int(mapRadius, mapRadius), new RoomConnection[]{});
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

        // twigCount, out Vector2Int[] twigs

        //based on this website: https://www.boristhebrave.com/2020/09/12/dungeon-generation-in-binding-of-isaac/
        /*private int[,] GetMapShape(int roomCount, int twigCount, out Vector2Int[] twigsPos, out Vector2Int endPos)
        {
            var roomAllowance = roomCount - twigCount;

            var mapShape = new int[MapSize + 2, MapSize + 2];

            mapShape[mapRadius, mapRadius] = 1; //Start with the middle room
            roomAllowance--;

            var i = mapRadius;
            var j = mapRadius;

            while (roomAllowance > 0)
            {
                //Look every room on the map
                // for (var i = 0; i < MapSize; i++)
                // {
                //     for (var j = 0; j < MapSize; j++)
                //     {
                //if (mapShape[i, j] != 1) continue;
                //Keep going if the room exists

                var neighborUp = mapShape[i, j + 1];
                var neighborDown = mapShape[i, j - 1];
                var neighborLeft = mapShape[i - 1, j];
                var neighborRight = mapShape[i + 1, j];

                //Check if a room should be generated up
                if (j < MapSize + 2 && neighborUp != 1)
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

                            j += 1;

                            roomAllowance--;
                            if (roomAllowance <= 0)
                            {
                                endPos = new Vector2Int(i, j);
                                continue; //return mapShape;
                            }
                        }
                    }
                }

                //Check if a room should be generated down
                if (j - 2 > 0 && neighborDown != 1)
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

                            j -= 1;

                            roomAllowance--;
                            if (roomAllowance <= 0)
                            {
                                endPos = new Vector2Int(i, j);
                                continue; //return mapShape;
                            }
                        }
                    }
                }

                //Check if a room should be generated left
                if (i - 2 > 0 && neighborLeft != 1)
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

                            i -= 1;

                            roomAllowance--;
                            if (roomAllowance <= 0)
                            {
                                endPos = new Vector2Int(i, j);
                                continue; //return mapShape;
                            }
                        }
                    }
                }

                //Check if a room should be generated right
                if (i + 2 < MapSize + 2 && neighborRight != 1)
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

                            i += 1;

                            roomAllowance--;
                            if (roomAllowance <= 0)
                            {
                                endPos = new Vector2Int(i, j);
                                continue; //return mapShape;
                            }
                        }
                    }
                }
                //     }
                // }
            }

            endPos = new Vector2Int(i, j);

            twigsPos = new Vector2Int[twigCount];

            var positions = new List<Vector2Int>();

            for (var x = 0; x < MapSize + 2; x++)
            {
                for (var y = 0; y < MapSize + 2; y++)
                {
                    if (mapShape[x, y] == 1) positions.Add(new Vector2Int(x, y));
                }
            }

            for (var t = 0; t < twigCount; t++)
            {
                var pos = GameTools.SelectRandom(positions);
                twigsPos[t] = GameTools.SelectRandom(GetOpenNeighborPositions(pos, mapShape)
                    .Where(e => GetOpenNeighborPositions(e, mapShape).Count == 3));
                positions.Remove(pos);
            }

            return mapShape;
        }*/

        private List<Vector2Int> GetOpenNeighborPositions(Vector2Int pos, Room[,] shapeMap)
        {
            List<Vector2Int> openPositions = new();

            if (pos.x + 1 < shapeMap.GetLength(0) && shapeMap[pos.x + 1, pos.y] == null) //Right
            {
                openPositions.Add(new Vector2Int(pos.x + 1, pos.y));
            }

            if (pos.x - 1 > 0 && shapeMap[pos.x - 1, pos.y] == null) //LEFT
            {
                openPositions.Add(new Vector2Int(pos.x - 1, pos.y));
            }

            if (pos.y + 1 < shapeMap.GetLength(1) && shapeMap[pos.x, pos.y + 1] == null) //Up
            {
                openPositions.Add(new Vector2Int(pos.x, pos.y + 1));
            }

            if (pos.y - 1 > 0 && shapeMap[pos.x, pos.y - 1] == null) //Down
            {
                openPositions.Add(new Vector2Int(pos.x, pos.y - 1));
            }

            return openPositions;
        }
    }
}