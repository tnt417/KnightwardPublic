using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TonyDev.Game.Core.Items.Money
{
    public class MoneyObject : MonoBehaviour, IHideable
    {
        [SerializeField] public Rigidbody2D rb2d;
        [SerializeField] private float attractRange;
        [SerializeField] private float attractSpeed;

        public static readonly Queue<KeyValuePair<GameObject, MoneyObject>> MoneyObjectPool = new();
        private const int MaxMoneyObjects = 500;
        private const int CombineRange = 5;

        public int amount;

        private void FixedUpdate()
        {
            var myPos = transform.position;
            var playerPos = Player.LocalInstance.transform.position;

            var distance = Vector2.Distance(myPos, playerPos);

            DoCombineLogic();

            if (distance > attractRange || Player.LocalInstance.CurrentParentIdentity != CurrentParentIdentity) return;

            rb2d.transform.Translate((playerPos - myPos).normalized * Mathf.Sqrt(attractRange - distance) *
                                     attractSpeed * Time.fixedDeltaTime);
        }

        private void DoCombineLogic()
        {
            var nearbyMoney = GetCombineable(MoneyObjectPool.Select(kv => kv.Value)
                .Where(obj => obj != null &&
                    obj.isActiveAndEnabled && obj.CurrentParentIdentity == CurrentParentIdentity &&
                    Vector2.Distance(transform.position, obj.transform.position) < CombineRange).ToList());

            if (nearbyMoney.FirstOrDefault() == null) return;

            var nearAmount = nearbyMoney.Sum(money => money.amount);

            foreach (var money in nearbyMoney)
            {
                money.gameObject.SetActive(false);
            }

            ObjectSpawner.SpawnMoney(nearAmount,
                GameTools.GetMeanVector(nearbyMoney.Select(mon => mon.transform.position).ToList()),
                nearbyMoney.First().CurrentParentIdentity);
        }

        private List<MoneyObject> GetCombineable(List<MoneyObject> unfiltered)
        {
            var combineto5 = unfiltered.Count(money => money.amount == 1) >= 5;
            var combineto25 = unfiltered.Count(money => money.amount == 5) >= 5;
            var combineto100 = unfiltered.Count(money => money.amount == 25) >= 4;

            return unfiltered.Where(money =>
                money.amount != 100 && ((money.amount == 1 && combineto5) || (money.amount == 5 && combineto25) &&
                (money.amount == 25 && combineto100))).ToList();
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
            ObjectSpawner.SpawnTextPopup(transform.position, "+" + amount, Color.yellow);
            GameManager.Money += amount;

            gameObject.SetActive(false);
        }

        public NetworkIdentity CurrentParentIdentity { get; set; }

        public static void SpawnMoney(GameObject prefab, Vector2 position, int amount, NetworkIdentity parentIdentity)
        {
            var takeFromPool = MoneyObjectPool.Count >= MaxMoneyObjects;

            var kvp = takeFromPool ? MoneyObjectPool.Dequeue() : new KeyValuePair<GameObject, MoneyObject>(null, null);

            var go = kvp.Key;
            var money = kvp.Value;

            var moneyObject = takeFromPool && go != null
                ? go
                : Instantiate(prefab);

            if (go != null && go.activeSelf)
            {
                money.OnPickup();
            }

            if (money == null || money.rb2d == null) money = moneyObject.GetComponent<MoneyObject>();

            kvp = new KeyValuePair<GameObject, MoneyObject>(moneyObject, money);

            // Money

            money.amount = amount;

            if (parentIdentity != null)
            {
                var room = RoomManager.Instance.GetRoomFromID(parentIdentity.netId);

                if (room != null)
                {
                    money.CurrentParentIdentity = parentIdentity;

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

            var rb2d = money.rb2d;
            rb2d.velocity = dir * 15f;

            MoneyObjectPool.Enqueue(kvp);
        }
    }
}