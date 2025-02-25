using System;
using System.Collections.Generic;
using System.Linq;
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

        [NonSerialized] public List<string> Unlocks = new();

        [NonSerialized] public List<ItemData> unlockedItems = new();
        [NonSerialized] private List<ItemData> _allItems = new();
        public static List<ItemData> UnlockedItems => Instance.unlockedItems;

        public const string UnlocksKey = "item-unlocks";

        public List<ItemData> defaultUnlocks;
        public List<ItemData> demoUnlocks;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            
            _allItems = Resources.LoadAll<ItemData>("Items").Select(Instantiate).ToList();

            DontDestroyOnLoad(gameObject);
            Instance = this;

            if (GameManager.IsDemo)
            {
                unlockedItems = demoUnlocks;
                return;
            }

            if (PlayerPrefs.HasKey(UnlocksKey))
            {
                Unlocks = JsonConvert.DeserializeObject<List<string>>(
                    PlayerPrefs.GetString(UnlocksKey));
            }
            
            if (Unlocks == null || Unlocks.Count == 0)
            {
                Unlocks = new List<string>();
            }

            UnlockAllItems(); //TODO: Comment this out if adding item unlocking
            
            // foreach (var u in defaultUnlocks)
            // {
            //     Unlocks.Add(u.item.itemName);
            // }

            GameManager.OnGameManagerAwake += InitItems;
        }

        private void InitItems()
        {

            foreach (var s in Unlocks)
            {
                var matchingItem = _allItems.FirstOrDefault(i => i != null && i.item.itemName == s);
                
                if (matchingItem == null) continue;
                
                unlockedItems.Add(_allItems.First(i => i != null && i.item.itemName == s));
            }
        }

        public List<ItemData> GetLockedItems()
        {
            return _allItems.Where(i => !Unlocks.Contains(i.item.itemName)).ToList();
        }
        
        public void AddUnlockSessionOnly(string itemName)
        {
            if (unlockedItems.Any(i => i.item.itemName == itemName)) return;

            var item = _allItems.FirstOrDefault(i => i.item.itemName == itemName);

            if (item != null)
            {
                unlockedItems.Add(item);
            }
        }
        
        public void AddUnlock(string itemName)
        {
            if (Unlocks.Contains(itemName) || GameManager.IsDemo) return;

            var item = _allItems.First(i => i.item.itemName == itemName);
            
            Unlocks.Add(itemName);
            
            unlockedItems.Add(item);
            
            GameConsole.Log("Unlocked item " + itemName + "!");
        }

        public void UnlockRandomItem()
        {
            var item = GameTools.SelectRandom(_allItems.Where(i => !Unlocks.Contains(i.item.itemName)));

            if (item == null) return;
            
            AddUnlock(item.item.itemName);
        }

        [GameCommand(Keyword = "unlockall", PermissionLevel = PermissionLevel.Cheat)]
        public static void UnlockAllItems()
        {
            foreach (var i in Instance._allItems)
            {
                Instance.AddUnlock(i.item.itemName);
            }
        }
        
        [GameCommand(Keyword = "clearunlocks", PermissionLevel = PermissionLevel.Cheat)]
        public static void ResetUnlocks()
        {
            Instance.Unlocks.Clear();
            Instance.unlockedItems.Clear();
            
            foreach (var u in Instance.defaultUnlocks)
            {
                Instance.Unlocks.Add(u.item.itemName);
            }

            Instance.InitItems();
        }

        private void OnApplicationQuit()
        {
            if(Unlocks is {Count: > 0}) PlayerPrefs.SetString(UnlocksKey, JsonConvert.SerializeObject(Unlocks));
        }
    }
}
