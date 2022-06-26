using UnityEngine;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomGenerator : MonoBehaviour
    {
        //Editor variables
        [SerializeField] public int mapRadius;
        [SerializeField] public Vector2 roomOffset;
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject[] generateRoomPrefabs;
        [SerializeField] private RoomManager roomManager;
        //
    
        public int MapSize => mapRadius * 2 + 1;
        private bool _generated = false;
        private int _seed;

        private void Awake()
        {
            _seed = Random.Range(0, 100000); //Get a random seed to generate the room based on.
        }
    
        public Room[,] Generate() //Returns an array of randomly generated rooms
        {
            if (_generated) return roomManager.Rooms;

            Random.InitState(_seed);

            var generateShape = GetMapShape(80f, 20f);
        
            var rooms = new Room[MapSize, MapSize];
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (generateShape[i, j] == 0) continue; //If the shape says not to generate, move on
                
                    GameObject prefab;
                
                    if (i == mapRadius && j == mapRadius) prefab = startRoomPrefab; //If at the center, generate the startRoomPrefab
                    else prefab = generateRoomPrefabs[Random.Range(0, generateRoomPrefabs.Length)]; //Otherwise, generate a random prefab from the list
                    if (prefab == null) continue; //Move on if no prefab is provided
                
                    //Instantiate a room prefab, set its index, deactivate it, and add it to the array.
                    var go = Instantiate(prefab, new Vector2(roomOffset.x * (i-mapRadius), roomOffset.y * (j-mapRadius)),Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
                    go.SendMessage("SetRoomIndex", new Vector2Int(i, j));
                    go.SetActive(false);
                    rooms[i, j] = go.GetComponent<Room>();
                    //
                }
            }
            rooms[mapRadius, mapRadius].gameObject.SetActive(true);
            _generated = true;
            return rooms;
        }

        private int[,] GetMapShape(float branchChance, float twigChance) //Returns a generated int[,] that will determine the shape of the map
        {
            var branchShape = new int[MapSize, MapSize]; //Map of 1s and 0s that will determine if a room is generated in a spot.

            //Branch out cardinals, with a chance to continue/end the branch every step.
            for (var x = mapRadius; x >= 0; x--) //LEFT BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[x, mapRadius] = 1;
                else{ branchShape[x, mapRadius] = 0; break; }
            }
        
            for (var x = mapRadius; x < MapSize; x++) //RIGHT BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[x, mapRadius] = 1;
                else{ branchShape[x, mapRadius] = 0; break; }
            }
        
            for (var y = mapRadius; y < MapSize; y++) //TOP BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[mapRadius, y] = 1;
                else{ branchShape[mapRadius, y] = 0; break; }
            }
        
            for (var y = mapRadius; y >= 0; y--) //BOTTOM BRANCH
            {
                if (Random.Range(0, 100) <= branchChance) branchShape[mapRadius, y] = 1;
                else{ branchShape[mapRadius, y] = 0; break; }
            }
            //
        
            branchShape[mapRadius, mapRadius] = 1; //Make start room generate, obviously

            var twigShape = new int[MapSize, MapSize]; //Separate array for the "twigs"
        
            //Chance to generate rooms adjacent to the branches (twigs)
            for (var i = 1; i < MapSize-1; i++)
            {
                for (var j = 1; j < MapSize-1; j++)
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
