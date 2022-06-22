using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    //Editor variables
    [SerializeField] private Grid grid;
    [SerializeField] private RoomDoor[] roomDoors;

    [SerializeField] private GameObject spawnPrefabOnClear;
    //

    private Vector2Int _roomIndex;
    private List<Direction> _openDirections;
    public Rect RoomRect => FindRoomRect();
    private bool _prefabSpawned = false;

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
            rd.open = true;
        }
    }

    private void FixedUpdate()
    {
        if (GetComponentInChildren<Enemy>() != null || GetComponentInChildren<EnemySpawner>() != null)
        {
            LockAllDoors(); //Lock doors while enemies are alive in the room.
        }
        else
        {
            if (!_prefabSpawned && spawnPrefabOnClear != null)
            {
                Instantiate(spawnPrefabOnClear, transform.position, quaternion.identity, transform); //Instantiate the on clear prefab in the center of the room
                _prefabSpawned = true;
            }

            SetOpenDirections(_openDirections); //Otherwise, open/close the doors as normal.
        }
    }

    private void LockAllDoors() //Closes all doors
    {
        foreach (var rd in roomDoors)
        {
            rd.open = false;
        }
    }

    public void SetRoomIndex(Vector2Int v2) //Called with event. Communicates where this room is on the grid.
    {
        _roomIndex = v2;
    }
}