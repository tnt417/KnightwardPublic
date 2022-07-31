using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    public class PickupSpawner : MonoBehaviour
    {
        private static PickupSpawner _instance;
        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private float moneyOutwardForce;

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        public static void SpawnMoney(int amount, Vector2 originPos)
        {
            for (var i = 0; i < amount; i++)
            {
                var angle = Random.Range(0, 360);

                var radAngle = angle * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
                
                var moneyObject = Instantiate(_instance.moneyPrefab, originPos, Quaternion.identity);

                var rb2d = moneyObject.GetComponent<Rigidbody2D>();
                rb2d.velocity = dir * _instance.moneyOutwardForce;
            }
        }
    }
}
