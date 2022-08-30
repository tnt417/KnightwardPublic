using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TonyDev.Game.Core.Items.Money
{
    public class MoneyObject : MonoBehaviour, IHideable
    {
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private float attractRange;
        [SerializeField] private float attractSpeed;

        private void FixedUpdate()
        {
            var myPos = transform.position;
            var playerPos = Player.LocalInstance.transform.position;

            var distance = Vector2.Distance(myPos, playerPos);

            if (distance > attractRange || Player.LocalInstance.CurrentParentIdentity != CurrentParentIdentity) return;

            rb2d.transform.Translate((playerPos - myPos).normalized * Mathf.Sqrt(attractRange - distance) * attractSpeed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Player>();
            if (player == null || !other.isTrigger || !player.isLocalPlayer || player.CurrentParentIdentity != CurrentParentIdentity) return;

            SoundManager.PlayRampingPitchSound("moneyPickup", transform.position);
            GameManager.Money += 1;
            enabled = false;
            Destroy(gameObject);
        }

        public NetworkIdentity CurrentParentIdentity { get; set; }
    }
}