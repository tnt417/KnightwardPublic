using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Vector2Int roomIndex;
    
    [SerializeField] private RoomDoor[] roomDoors;
    public Rect RoomRect => FindRoomRect();
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

        return new Rect(transform.position.x + xMin, transform.position.y + yMin, xMax - xMin, yMax - yMin);
    }

    public void SetOpenDirections(List<Direction> directions)
    {
        foreach (var d in directions)
        {
            foreach (var rd in roomDoors)
            {
                if (rd.Direction == d) rd.open = true;
            }
        }
    }

    public void SetRoomIndex(Vector2Int v2) //Called with event
    {
        roomIndex = v2;
    }
}
