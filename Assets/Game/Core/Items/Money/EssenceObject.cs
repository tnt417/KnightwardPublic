using System;
using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Items.Money
{
    public class EssenceObject : MonoBehaviour, IHideable
    {
        [SerializeField] public Rigidbody2D rb2d;
        [SerializeField] private float attractRange;
        [SerializeField] private float attractSpeed;

        private static readonly Queue<KeyValuePair<GameObject, EssenceObject>> EssenceObjectPool = new();
        private const int MaxEssenceObjects = 500;

        public int amount;

        private void FixedUpdate()
        {
            var myPos = transform.position;
            var playerPos = Player.LocalInstance.transform.position;

            var distance = Vector2.Distance(myPos, playerPos);

            if (distance > attractRange || Player.LocalInstance.CurrentParentIdentity != CurrentParentIdentity) return;

            rb2d.transform.Translate((playerPos - myPos).normalized * Mathf.Sqrt(attractRange - distance) *
                                     attractSpeed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var player = other.GetComponent<Player>();
            if (player == null || !other.isTrigger || !player.isLocalPlayer ||
                player.CurrentParentIdentity != CurrentParentIdentity) return;

            OnPickup();
        }

        public void OnPickup()
        {
            SoundManager.PlayRampingPitchSound("moneyPickup", transform.position);
            ObjectSpawner.SpawnTextPopup(transform.position, "+" + amount, Color.cyan);
            GameManager.Essence += amount;

            gameObject.SetActive(false);
        }

        public NetworkIdentity CurrentParentIdentity { get; set; }

        public static void SpawnEssence(GameObject prefab, Vector2 position, int amount, NetworkIdentity parentIdentity)
        {
            var takeFromPool = EssenceObjectPool.Count >= MaxEssenceObjects;

            var kvp = takeFromPool ? EssenceObjectPool.Dequeue() : new KeyValuePair<GameObject, EssenceObject>(null, null);

            var go = kvp.Key;
            var essence = kvp.Value;
            
            var moneyObject = takeFromPool && go != null
                ? go
                : Instantiate(prefab);

            if (go != null && go.activeSelf)
            {
                essence.OnPickup();
            }

            if (essence == null || essence.rb2d == null) essence = moneyObject.GetComponent<EssenceObject>();

            kvp = new KeyValuePair<GameObject, EssenceObject>(moneyObject, essence);

            // Money
            
            essence.amount = amount;

            if (parentIdentity != null)
            {
                var room = RoomManager.Instance.GetRoomFromID(parentIdentity.netId);

                if (room != null)
                {
                    essence.CurrentParentIdentity = parentIdentity;

                    room.roomChildObjects.Add(moneyObject);
                }
            }
            
            // Transform
            
            moneyObject.transform.position = position;
            
            moneyObject.transform.localScale = amount switch
            {
                1 => Vector3.one,
                5 => Vector3.one * 1.2f,
                25 => Vector3.one * 1.4f,
                100 => Vector3.one * 1.6f,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            moneyObject.SetActive(true);
            
            // Outward movement
            
            var angle = Random.Range(0, 360);

            var radAngle = angle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));

            var rb2d = essence.rb2d;
            rb2d.velocity = dir * 15f;

            EssenceObjectPool.Enqueue(kvp);
        }
    }
}
