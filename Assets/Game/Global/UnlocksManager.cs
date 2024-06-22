using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Newtonsoft.Json;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Console;
using UnityEngine;
using UnityEngine.Serialization;

namespace TonyDev
{
    public class UnlocksManager : MonoBehaviour
    {
        public static UnlocksManager Instance;

        [NonSerialized] public HashSet<string> Unlocks = new();

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


            if (PlayerPrefs.HasKey(UnlocksKey))
            {
                Unlocks = JsonConvert.DeserializeObject<HashSet<string>>(
                    PlayerPrefs.GetString(UnlocksKey));
            }
            
            if (Unlocks == null || Unlocks.Count == 0)
            {
                Unlocks = new HashSet<string>();
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
                var matchingItem = GameManager.AllItems.FirstOrDefault(i => i != null && i.item.itemName == s);
                
                if (matchingItem == null) continue;
                
                unlockedItems.Add(GameManager.AllItems.First(i => i != null && i.item.itemName == s));
            }
        }

        public void AddUnlockSessionOnly(string itemName)
        {
            if (unlockedItems.Any(i => i.item.itemName == itemName)) return;
            
            unlockedItems.Add(GameManager.AllItems.First(i => i.item.itemName == itemName));
        }
        
        public void AddUnlock(string itemName)
        {
            if (Unlocks.Contains(itemName)) return;

            var item = GameManager.AllItems.First(i => i.item.itemName == itemName);
            
            Unlocks.Add(itemName);
            
            unlockedItems.Add(item);
            
            GameConsole.Log("Unlocked item " + itemName + "!");
        }

        public void UnlockRandomItem()
        {
            var item = GameTools.SelectRandom(GameManager.AllItems.Where(i => !Unlocks.Contains(i.item.itemName)));

            if (item == null) return;
            
            AddUnlock(item.item.itemName);
        }

        [GameCommand(Keyword = "unlockall", PermissionLevel = PermissionLevel.Cheat)]
        public static void UnlockAllItems()
        {
            foreach (var i in GameManager.AllItems)
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
