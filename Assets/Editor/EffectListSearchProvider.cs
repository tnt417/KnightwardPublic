using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Items;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TonyDev.Editor
{
    public class EffectListSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private Type[] _listItems;
        private Action<Type> _onSetIndexCallback;

        public EffectListSearchProvider(Type[] items, Action<Type> callback)
        {
            _listItems = items;
            _onSetIndexCallback = callback;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchList = new List<SearchTreeEntry>();

            searchList.Add(new SearchTreeGroupEntry(new GUIContent("Effects"), 0));

            searchList.AddRange(from type in _listItems let typeName = type.ToString().Split('.').Last() select new SearchTreeEntry(new GUIContent(typeName)) {userData = type, level = 1,});

            return searchList;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _onSetIndexCallback?.Invoke((Type) searchTreeEntry.userData);
            return true;
        }
    }
}