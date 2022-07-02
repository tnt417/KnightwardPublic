using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomGenerator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public int mapRadius;
        [SerializeField] public Vector2 roomOffset;
        [SerializeField] private GameObject[] generateRoomPrefabs;
        [SerializeField] private GameObject[] guaranteedGeneratePrefabs;
        [SerializeField] private RoomManager roomManager;
        //

        public Vector2Int startingRoomPos = Vector2Int.zero;
        public int MapSize => mapRadius * 2 + 1;
        private bool _generated = false;
        private int _seed;

        private void Awake()
        {
            _seed = Random.Range(0, 100000); //Get a random seed to generate the room based on.
        }

        public void Reset()
        {
            _generated = false;
            _seed = Random.Range(0, 100000);
        }

        public Room[,] Generate() //Returns an array of randomly generated rooms
        {
            if (_generated) return roomManager.Rooms;

            Random.InitState(_seed);

            var generateShape = GetMapShape(80f, 20f);

            var rooms = new Room[MapSize, MapSize];
            var prefabs = new GameObject[MapSize, MapSize];

            for (var i = 0; i < MapSize; i++)
            {
                for (var j = 0; j < MapSize; j++)
                {
                    if (generateShape[i, j] == 0) continue; //If the shape says not to generate, move on
                    prefabs[i, j] =
                        generateRoomPrefabs
                            [Random.Range(0, generateRoomPrefabs.Length)]; //Otherwise, generate a random prefab from the list
                }
            }

            List<Vector2Int> validReplaces = new List<Vector2Int>();

            //Generate guaranteed rooms
            for (var i = 0; i < MapSize; i++)
            for (var j = 0; j < MapSize; j++)
                if(generateShape[i,j] == 1) validReplaces.Add(new Vector2Int(i, j));

            foreach (var go in guaranteedGeneratePrefabs)
            {
                var replaceAt = Tools.SelectRandom(validReplaces);
                prefabs[replaceAt.x, replaceAt.y] = go;
                validReplaces.Remove(replaceAt);
                if (go.CompareTag("StartRoom"))
                    startingRoomPos = replaceAt;
            }

            for (var i = 0; i < MapSize; i++) //Generate every room
            for (var j = 0; j < MapSize; j++)
                rooms[i, j] = GenerateRoom(new Vector2Int(i, j), prefabs[i, j]);

            rooms[startingRoomPos.x, startingRoomPos.y].gameObject.SetActive(true);
            _generated = true;
            return rooms;
        }

        private Room GenerateRoom(Vector2Int index, GameObject prefab)
        {
            if (prefab == null) return null;
            
            //Instantiate a room prefab, set its index, deactivate it, and add it to the array.
            var go = Instantiate(prefab,
                new Vector2(roomOffset.x * (index.x - mapRadius), roomOffset.y * (index.y - mapRadius)),
                Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
            go.SendMessage("SetRoomIndex", new Vector2Int(index.x, index.y));
            go.SetActive(false);
            return go.GetComponent<Room>();
            //
        }

        private int[,]
            GetMapShape(float branchChance,
                float twigChance) //Returns a generated int[,] that will determine the shape of the map
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
        }
    }
}