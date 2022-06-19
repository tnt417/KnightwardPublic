using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Serialization;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private Room currentActiveRoom;
    [SerializeField] private RoomGenerator roomGenerator;
    
    public static RoomManager Instance;
    private SmoothCameraFollow _smoothCameraFollow;
    private Vector2Int _currentActiveRoomIndex;
    public Room[,] Rooms { get; private set; }
    public static bool InRoomsPhase => SceneManager.GetActiveScene().name == "RoomScene";

    private void Awake()
    {
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this);
    }
    private void Start()
    {
        if (!InRoomsPhase) return;
        _smoothCameraFollow = FindObjectOfType<SmoothCameraFollow>();
        StartRoomPhase();
    }
    private void StartRoomPhase()
    {
        Rooms = roomGenerator.Generate();
        DoDoorClosing();
        SetActiveRoom(roomGenerator.mapRadius, roomGenerator.mapRadius);
        var playerTransform = Player.Instance.transform;
        var currentPlayerPos = playerTransform.position;
        playerTransform.position = new Vector3(0f, 0f, currentPlayerPos.z);
    }

    public void ShiftActiveRoom(Direction direction)
    {
        var dx = 0;
        var dy = 0;
        
        switch (direction)
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
        SetActiveRoom(_currentActiveRoomIndex.x + dx, _currentActiveRoomIndex.y + dy);
        
        var playerTransform = Player.Instance.transform;
        playerTransform.transform.Translate(dx * 3, dy * 3, 0);
    }

    private void SetActiveRoom(int x, int y)
    {
        var newRoom = Rooms[x, y];
        if (newRoom == null) return;
        
        if(currentActiveRoom != null) currentActiveRoom.gameObject.SetActive(false);
        currentActiveRoom = newRoom;
        currentActiveRoom.gameObject.SetActive(true);
        _smoothCameraFollow.CameraBounds = currentActiveRoom.RoomRect;
        _currentActiveRoomIndex = new Vector2Int(x, y);
    }

    private void DoDoorClosing()
    {
        for (var i = 0; i < roomGenerator.MapSize; i++)
        {
            for (var j = 0; j < roomGenerator.MapSize; j++)
            {
                if (Rooms[i, j] == null) continue;
                var openDirections = new List<Direction>();
                if(CheckIfRoomAtPos(i-1, j)) openDirections.Add(Direction.Left);
                if(CheckIfRoomAtPos(i+1, j)) openDirections.Add(Direction.Right);
                if(CheckIfRoomAtPos(i, j-1)) openDirections.Add(Direction.Down);
                if(CheckIfRoomAtPos(i, j+1)) openDirections.Add(Direction.Up);
                Rooms[i, j].SetOpenDirections(openDirections);
            }
        }
    }

    private bool CheckIfRoomAtPos(int x, int y)
    {
        if (x < 0 || y < 0 || x > roomGenerator.MapSize || y > roomGenerator.MapSize) return false;
        return Rooms[x, y] != null;
    }
}
