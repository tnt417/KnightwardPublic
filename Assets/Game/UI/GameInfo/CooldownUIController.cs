using System;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev.Game.UI.GameInfo
{
    public class CooldownUIController : MonoBehaviour
    {
        private static CooldownUIController _instance;

        [SerializeField] private GameObject cooldownObjectPrefab;
        
        private Dictionary<int, CooldownObjectUI> _cooldownObjects = new();

        private void Awake()
        {
            _instance = this;
        }

        private static int _index;
        
        public static int RegisterCooldown(Sprite icon, Func<float> totalCooldown, Func<float> remainingCooldown, KeyCode activateKey)
        {
            var go = Instantiate(_instance.cooldownObjectPrefab, _instance.transform);
            var cd = go.GetComponent<CooldownObjectUI>();
            
            cd.Set(icon, totalCooldown, remainingCooldown, activateKey);

            _index++;
            
            _instance._cooldownObjects.Add(_index, cd);

            return _index;
        }
        
        public static void RemoveCooldown(int id)
        {
            CooldownObjectUI cd;

            if (!_instance._cooldownObjects.TryGetValue(id, out cd)) return;
            
            _instance._cooldownObjects.Remove(id);
            Destroy(cd.gameObject);
        }
    }
}
