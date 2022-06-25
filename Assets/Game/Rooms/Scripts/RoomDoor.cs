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
    //Editor variables
    [SerializeField] private GameObject wallObject;
    public Direction direction;
    public bool open;
    //

    private void Update()
    {
        wallObject.SetActive(!open); //If the door is locked, activate the wall.
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(open && other.GetComponent<Player>() != null && other.isTrigger) RoomManager.Instance.ShiftActiveRoom(direction); //Shift room when stepped on.
    }
}
