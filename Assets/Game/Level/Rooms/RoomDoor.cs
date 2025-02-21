using System.Linq;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.Level.Rooms
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class RoomDoor : MonoBehaviour
    {
        //Editor variables
        public Tilemap wallTilemap;
        public Direction direction;
        private BoxCollider2D _trigger;
        private BoxCollider2D _collider;

        [SerializeField] private Animator animator;

        [SerializeField] private ParticleSystem particles;
        //

        public bool IsOpen { get; private set; }

        public void Awake()
        {
            _collider = GetComponents<BoxCollider2D>().FirstOrDefault(bc => !bc.isTrigger);
            _trigger = GetComponents<BoxCollider2D>().FirstOrDefault(bc => bc.isTrigger);
        }

        public void SetOpen(bool open)
        {
            // foreach (var pos in wallTileSpots)
            // {
            //     wallTilemap.SetTile((Vector3Int) pos, open ? null : doorTile);
            // }
            animator.Play(open ? "GateOpen" : "GateClose");

            //wallTilemap.CompressBounds();

            IsOpen = open;
            _collider.enabled = !open;
            _trigger.enabled = open;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_hostInterestVisibility && NetworkClient.activeHost) return;
            var player = other.GetComponent<Player>();
            if (player != null && player == Player.LocalInstance && other.isTrigger && IsOpen &&
                player.CurrentParentIdentity == GetComponentInParent<NetworkIdentity>())
            {
                RoomManager.Instance.ShiftActiveRoom(direction); //Shift room when stepped on.
            }
        }

        private bool _hostInterestVisibility;

        [ServerCallback]
        public void SetHostVisibility(bool visible)
        {
            _hostInterestVisibility = visible;
            _collider.enabled = IsOpen && visible;
        }

        public void PlayParticles()
        {
            particles.Play();
        }

        // public void OnDrawGizmos()
        // {
        //     for (var i = 0; i < wallTileSpots.Length; i++)
        //     {
        //         var pos = wallTileSpots[i];
        //         Gizmos.color = new Color(1, 0, 0, 0.5f);
        //         Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector3.one);
        //     }
        // }
    }
}