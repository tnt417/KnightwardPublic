using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    Up, Down, Left, Right
}
public class RoomDoor : MonoBehaviour
{
    public Direction Direction { get; private set; }
    [SerializeField] private GameObject wallObject;
    public bool open;

    private void Update()
    {
        wallObject.SetActive(!open);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(open && other.GetComponent<Player>() != null && other.isTrigger) RoomManager.Instance.ShiftActiveRoom(Direction);
    }
}
