using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev
{
    public class Attractable : MonoBehaviour, IHideable
    {
        [SerializeField] public Rigidbody2D rb2d;
        [SerializeField] private float attractRange;
        [SerializeField] private float attractSpeed;

        private void Awake()
        {
            CurrentParentIdentity = GetComponentInParent<Room>().netIdentity;
        }

        private void FixedUpdate()
        {
            var myPos = transform.position;
            var playerPos = Player.LocalInstance.transform.position;

            var distance = Vector2.Distance(myPos, playerPos);

            if (distance > attractRange || Player.LocalInstance.CurrentParentIdentity != CurrentParentIdentity) return;

            rb2d.transform.Translate((playerPos - myPos).normalized * Mathf.Sqrt(attractRange - distance) *
                                     attractSpeed * Time.fixedDeltaTime);
        }

        public NetworkIdentity CurrentParentIdentity { get; set; }
    }
}
