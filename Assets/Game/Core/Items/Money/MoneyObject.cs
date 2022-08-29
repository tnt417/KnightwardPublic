using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TonyDev.Game.Core.Items.Money
{
    public class MoneyObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private float attractRange;
        [SerializeField] private float attractSpeed;

        private void FixedUpdate()
        {
            var myPos = transform.position;
            var playerPos = Player.LocalInstance.transform.position;

            var distance = Vector2.Distance(myPos, playerPos);

            if (distance > attractRange) return;

            rb2d.transform.Translate((playerPos - myPos).normalized * Mathf.Sqrt(attractRange - distance) * attractSpeed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Player>();
            if (player == null || !player.isLocalPlayer) return;
            
            SoundManager.PlayRampingPitchSound("moneyPickup", transform.position);
            GameManager.Money += 1;
            Destroy(gameObject);
        }
    }
}