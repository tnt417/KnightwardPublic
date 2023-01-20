using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Newtonsoft.Json;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;

namespace TonyDev
{
    public class UnlocksManager : MonoBehaviour
    {
        public static UnlocksManager Instance;

        public Dictionary<string, ItemData> Unlocks = new();

        [NonSerialized] public List<ItemData> unlockedItems = new();
        public static List<ItemData> UnlockedItems => Instance.unlockedItems;

        public const string UnlocksKey = "item-unlocks";

        public List<ItemData> defaultUnlocks;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            Unlocks = JsonConvert.DeserializeObject<Dictionary<string, ItemData>>(
                PlayerPrefs.GetString(UnlocksKey));
            
            if (Unlocks == null || Unlocks.Count == 0)
            {
                Unlocks = new Dictionary<string, ItemData>();
                
                foreach (var u in defaultUnlocks)
                {
                    Unlocks.Add(u.name, u);
                }
            }

            foreach (var kv in Unlocks)
            {
                unlockedItems.Add(kv.Value);
            }
        }

        public void AddUnlockSessionOnly(string itemName)
        {
            if (unlockedItems.Any(i => i.item.itemName == itemName)) return;
            
            unlockedItems.Add(GameManager.AllItems.First(i => i.item.itemName == itemName));
        }
        
        public void AddUnlock(string itemName)
        {
            if (Unlocks.ContainsKey(itemName)) return;

            var item = GameManager.AllItems.First(i => i.item.itemName == itemName);
            
            Unlocks.Add(itemName, item);
            
            unlockedItems.Add(item);
            
            GameConsole.Log("Unlocked item " + itemName + "!");
        }

        public void UnlockRandomItem()
        {
            var item = Tools.SelectRandom(GameManager.AllItems.Where(i => !Unlocks.ContainsKey(i.item.itemName)));
            
            AddUnlock(item.item.itemName);
        }
        
        private void OnApplicationQuit()
        {
            if(Unlocks is {Count: > 0}) PlayerPrefs.SetString(UnlocksKey, JsonConvert.SerializeObject(Unlocks));
        }
    }
}
