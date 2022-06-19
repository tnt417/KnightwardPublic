using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] public int mapRadius;
    public int MapSize => mapRadius * 2 + 1;
    [SerializeField] public Vector2 roomOffset;
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject[] generateRoomPrefabs;
    [SerializeField] private RoomManager roomManager;
    private bool _generated = false;
    private int _seed;

    private void Awake()
    {
        _seed = 6969;
    }

    public Room[,] Generate()
    {
        if (_generated) return roomManager.Rooms;
        
        Random.InitState(_seed);
        var rooms = new Room[MapSize, MapSize];
        for (int i = 0; i < MapSize; i++)
        {
            for (int j = 0; j < MapSize; j++)
            {
                GameObject prefab;
                if (i == mapRadius && j == mapRadius) prefab = startRoomPrefab;
                else prefab = generateRoomPrefabs[Random.Range(0, generateRoomPrefabs.Length)];
                var go = Instantiate(prefab, new Vector2(roomOffset.x * (i-mapRadius), roomOffset.y * (j-mapRadius)),Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
                go.SendMessage("SetRoomIndex", new Vector2Int(i, j));
                go.SetActive(false);
                rooms[i, j] = go.GetComponent<Room>();
            }
        }
        rooms[mapRadius, mapRadius].gameObject.SetActive(true);
        _generated = true;
        return rooms;
    }
}
