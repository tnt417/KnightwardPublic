using System;
using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TonyDev
{
    public class CreateItemWindow : EditorWindow
    {
        [MenuItem("Assets/Create/Item")]
        public static void ShowWindow () 
        {
            GetWindow(typeof(CreateItemWindow));
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Create Item");
            maxSize = new Vector2(450f, 145f);
            minSize = maxSize;
        }

        private ItemType itemType;
        private ItemRarity itemRarity;
        private Sprite itemSprite;
        private string itemName;
        
        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Item Creation", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            //Item name
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Item Name", EditorStyles.boldLabel);
            
            itemName = EditorGUILayout.TextField(itemName);
            
            EditorGUILayout.EndHorizontal();

            //Item type
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Item Type", EditorStyles.boldLabel);
            
            itemType = (ItemType)EditorGUILayout.EnumPopup(itemType);
            
            EditorGUILayout.EndHorizontal();
            
            //Item rarity
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Item Rarity", EditorStyles.boldLabel);
            
            itemRarity = (ItemRarity)EditorGUILayout.EnumPopup(itemRarity);
            
            EditorGUILayout.EndHorizontal();

            //Item sprite
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Item Sprite", EditorStyles.boldLabel);
            
            itemSprite = (Sprite)EditorGUILayout.ObjectField(itemSprite, typeof(Sprite), false);
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create"))
            {
                AssetDatabase.OpenAsset(Create());
                Close();
            }
        }

        private ItemData Create()
        {
            var id = CreateInstance<ItemData>();

            var parentFolder = "Assets/Game/Core/Items/" + Enum.GetName(typeof(ItemType), itemType);
            
            AssetDatabase.CreateFolder(parentFolder, itemName);
            AssetDatabase.CreateAsset(id, parentFolder + "/" + itemName + "/" + itemName + "Data.asset");
            
            id.item.itemName = itemName;
            id.item.itemType = itemType;
            id.item.uiSprite = itemSprite;
            id.item.itemRarity = itemRarity;
            
            return id;
        }
    }
}
