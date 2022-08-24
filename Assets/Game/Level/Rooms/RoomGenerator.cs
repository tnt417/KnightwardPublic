using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using PlasticPipe.PlasticProtocol.Messages;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms
{
    [Serializable]
    public struct Map
    {
        public Map(Room[,] rooms, Vector2Int startingRoomPos)
        {
            Rooms = rooms;
            StartingRoomPos = startingRoomPos;
        }
        
        public readonly Room[,] Rooms;
        public readonly Vector2Int StartingRoomPos;
        public Room StartingRoom => Rooms[StartingRoomPos.x, StartingRoomPos.y];
    }
    public class RoomGenerator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public int mapRadius;
        [SerializeField] public Vector2 roomOffset;
        [SerializeField] private GameObject[] generateRoomPrefabs;
        [SerializeField] private GameObject[] guaranteedGeneratePrefabs;

        [SerializeField] private RoomManager roomManager;
        //

        public Vector2Int startingRoomPos;
        public int MapSize => mapRadius * 2 + 1;
        private bool _generated = false;
        private int _seed;

        private void Awake()
        {
            startingRoomPos = new Vector2Int(mapRadius, mapRadius);
            _seed = Random.Range(0, 100000); //Get a random seed to generate the room based on.
            
            var prefabs = generateRoomPrefabs.ToList();

            prefabs.AddRange(guaranteedGeneratePrefabs);
            
            foreach (var prefab in prefabs)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        public void Reset()
        {
            _generated = false;
            _seed = Random.Range(0, 100000);
        }
        
        [Server]
        public Map Generate() //Returns an array of randomly generated rooms
        {
            if (!GameManager.Instance.isServer) return default;
            
            if (_generated) return roomManager.map;

            Random.InitState(_seed);

            var roomCount = Random.Range(0, 2) + 7;

            var generateShape = GetMapShape(roomCount);

            var rooms = new Room[MapSize, MapSize];
            var prefabs = new GameObject[MapSize, MapSize];

            for (var i = 0; i < MapSize; i++)
            {
                for (var j = 0; j < MapSize; j++)
                {
                    if (generateShape[i, j] == 0) continue; //If the shape says not to generate, move on

                    prefabs[i, j] =
                        GetRandomValidPrefab(new Vector2Int(i, j),
                            generateShape); //Otherwise, generate a random prefab from the list
                }
            }

            var validReplaces = new List<Vector2Int>();

            //Generate guaranteed rooms
            for (var i = 0; i < MapSize; i++)
            for (var j = 0; j < MapSize; j++)
                if (generateShape[i, j] == 1)
                    validReplaces.Add(new Vector2Int(i, j));

            foreach (var go in guaranteedGeneratePrefabs)
            {
                var replaceAt = go.CompareTag("StartRoom") ? startingRoomPos : Tools.SelectRandom(validReplaces);
                prefabs[replaceAt.x, replaceAt.y] = go;
                validReplaces.Remove(replaceAt);
            }

            for (var i = 0; i < MapSize; i++) //Generate every room
            for (var j = 0; j < MapSize; j++)
                rooms[i, j] = GenerateRoom(new Vector2Int(i, j), prefabs[i, j]);

            rooms[startingRoomPos.x, startingRoomPos.y].gameObject.SetActive(true);
            _generated = true;
            return new Map(rooms, startingRoomPos);
        }

        private GameObject GetRandomValidPrefab(Vector2Int index, int[,] shape)
        {
            var neededDoors = new List<Direction>();

            if ((index.y + 1 < shape.GetLength(1) ? shape[index.x, index.y + 1] : 0) == 1)
                neededDoors.Add(Direction.Up);
            if ((index.y - 1 > 0 ? shape[index.x, index.y - 1] : 0) == 1)
                neededDoors.Add(Direction.Down);
            if ((index.x - 1 > 0 ? shape[index.x - 1, index.y] : 0) == 1)
                neededDoors.Add(Direction.Left);
            if ((index.x + 1 < shape.GetLength(0) ? shape[index.x + 1, index.y] : 0) == 1)
                neededDoors.Add(Direction.Right);

            var validPrefabs = generateRoomPrefabs.Where(r =>
                neededDoors.TrueForAll(nd => r.GetComponent<Room>().GetDoorDirections().Contains(nd)));

            var prefab = Tools.SelectRandom(validPrefabs);

            return prefab;
        }

        private Room GenerateRoom(Vector2Int index, GameObject prefab)
        {
            if (prefab == null) return null;

            //Instantiate a room prefab, set its index, deactivate it, and add it to the array.
            var go = Instantiate(prefab,
                new Vector2(roomOffset.x * (index.x - mapRadius), roomOffset.y * (index.y - mapRadius)),
                Quaternion.identity);
            go.SendMessage("SetRoomIndex", new Vector2Int(index.x, index.y));

            NetworkServer.Spawn(go);

            return go.GetComponent<Room>();
            //
        }

        /*private int[,] GetMapShape(float roomCount) //Returns a generated int[,] that will determine the shape of the map
        {
            var branchShape =
                new int[MapSize, MapSize]; //Map of 1s and 0s that will determine if a room is generated in a spot.

            //Branch out cardinals, with a chance to continue/end the branch every step.
            for (var x = mapRadius; x >= 0; x--) //LEFT BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[x, mapRadius] = 1;
                else
                {
                    branchShape[x, mapRadius] = 0;
                    break;
                }
            }

            for (var x = mapRadius; x < MapSize; x++) //RIGHT BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[x, mapRadius] = 1;
                else
                {
                    branchShape[x, mapRadius] = 0;
                    break;
                }
            }

            for (var y = mapRadius; y < MapSize; y++) //TOP BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[mapRadius, y] = 1;
                else
                {
                    branchShape[mapRadius, y] = 0;
                    break;
                }
            }

            for (var y = mapRadius; y >= 0; y--) //BOTTOM BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[mapRadius, y] = 1;
                else
                {
                    branchShape[mapRadius, y] = 0;
                    break;
                }
            }
            //

            branchShape[mapRadius, mapRadius] = 1; //Make start room generate, obviously

            var twigShape = new int[MapSize, MapSize]; //Separate array for the "twigs"

            //Chance to generate rooms adjacent to the branches (twigs)
            for (var i = 1; i < MapSize - 1; i++)
            {
                for (var j = 1; j < MapSize - 1; j++)
                {
                    if (branchShape[i, j] == 0) continue;
                    if (Random.Range(0, 100) <= twigChance) twigShape[i + 1, j] = 1;
                    if (Random.Range(0, 100) <= twigChance) twigShape[i - 1, j] = 1;
                    if (Random.Range(0, 100) <= twigChance) twigShape[i, j + 1] = 1;
                    if (Random.Range(0, 100) <= twigChance) twigShape[i, j - 1] = 1;
                }
            }
            //

            //Add the twig shape to the branch shape...
            for (var i = 0; i < MapSize; i++)
            {
                for (var j = 0; j < MapSize; j++)
                {
                    if (twigShape[i, j] == 1) branchShape[i, j] = 1;
                }
            }
            //

            return branchShape; //...and return the final shape array
        }*/

        //based on this website: https://www.boristhebrave.com/2020/09/12/dungeon-generation-in-binding-of-isaac/
        private int[,] GetMapShape(float roomCount)
        {
            var roomAllowance = roomCount;

            var mapShape = new int[MapSize+2, MapSize+2];

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