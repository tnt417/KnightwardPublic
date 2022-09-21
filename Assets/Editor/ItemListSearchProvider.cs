using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Items;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TonyDev.Editor
{
    public class ItemListSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private ItemData[] _listItems;
        private Action<ItemData> _onSetIndexCallback;
        
        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for(int i =0;i<guids.Length;i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
 
            return a;
        }

        public ItemListSearchProvider(ItemData[] items, Action<ItemData> callback)
        {
            _listItems = items;
            _onSetIndexCallback = callback;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchList = new List<SearchTreeEntry>();

            searchList.Add(new SearchTreeGroupEntry(new GUIContent("Items"), 0));

            List<string> groups = new();

            foreach (var itemData in _listItems.OrderBy(id => id.item.itemType))
            {
                if (itemData.item == null) continue; //TODO might want to instead create an item for it
                
                var typeName = Enum.GetName(typeof(ItemType), itemData.item.itemType);

                if (!groups.Contains(typeName))
                {
                    groups.Add(typeName);
                    searchList.Add(new SearchTreeGroupEntry(new GUIContent(typeName), 1));
                }

                var itemEntry = new SearchTreeEntry(
                    new GUIContent(itemData.item.itemName,
                        (itemData != null && itemData.item != null && itemData.item.uiSprite != null)
                            ? itemData.item.uiSprite.texture
                            : null))
                {
                    userData = itemData,
                    level = 2,
                };
                searchList.Add(itemEntry);
            }

            return searchList;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _onSetIndexCallback?.Invoke((ItemData) searchTreeEntry.userData);
            return true;
        }
    }
}