using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TonyDev.Game.UI.Popups
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private int objectPoolSize;
        private static PopupManager _instance;
        private static readonly Queue<GameObject> PopupObjectPool = new ();

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        [SerializeField] private GameObject damageNumberPrefab;

        public static void SpawnPopup(Vector2 position, int damage, bool critical)
        {
            var go = PopupObjectPool.Count >= _instance.objectPoolSize ? PopupObjectPool.Dequeue() : Instantiate(_instance.damageNumberPrefab, position, Quaternion.identity);

            go.transform.position = position;
            
            var tmp = go.GetComponentInChildren<TextMeshPro>();
            var anim = go.GetComponent<Animator>();
            
            anim.Play("DamagePopup");

            tmp.text = "-" + damage;
            tmp.color = critical ? Color.red : Color.white;
            
            PopupObjectPool.Enqueue(go);
        }
    }
}
