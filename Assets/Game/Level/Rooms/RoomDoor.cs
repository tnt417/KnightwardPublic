using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.Level.Rooms
{
    public enum Direction
    {
        Up, Down, Left, Right
    }
    public class RoomDoor : MonoBehaviour
    {
        //Editor variables
        public Vector2Int[] wallTileSpots;
        public Tilemap wallTilemap;
        public TileBase doorTile;
        public Direction direction;
        private BoxCollider2D _collider;
        //

        public bool open;
        
        public void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }
        
        public void Open()
        {
            foreach (var pos in wallTileSpots)
            {
                wallTilemap.SetTile((Vector3Int) pos, null);
            }

            open = true;
            _collider.enabled = open;
        }

        public void Close()
        {
            foreach (var pos in wallTileSpots)
            {
                wallTilemap.SetTile((Vector3Int) pos, doorTile);   
            }

            open = false;
            _collider.enabled = open;
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_hostInterestVisibility && NetworkClient.isHostClient) return;
            if(other.GetComponent<Player>() == Player.LocalInstance && other.isTrigger) RoomManager.Instance.ShiftActiveRoom(direction); //Shift room when stepped on.
        }

        private bool _hostInterestVisibility;

        [Server]
        public void SetHostVisibility(bool visible)
        {
            _hostInterestVisibility = visible;
            _collider.enabled = open && visible;
        }
        
        public void OnDrawGizmos()
        {
            for (var i = 0; i < wallTileSpots.Length; i++)
            {
                var pos = wallTileSpots[i];
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector3.one);
            }
        }
    }
}