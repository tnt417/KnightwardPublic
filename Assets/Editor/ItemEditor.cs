using TonyDev.Game.Core.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TonyDev.Editor
{
    [CustomEditor(typeof(ItemData))]
    public class ItemEditor : UnityEditor.Editor
    {
        public ItemType itemType;

        // The function that makes the custom editor work
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            
            var sp = serializedObject.FindProperty("item");
            itemType = (ItemType) EditorGUILayout.EnumPopup("Type", (ItemType) sp.FindPropertyRelative(nameof(Item.itemType)).enumValueIndex);

            sp.FindPropertyRelative(nameof(Item.itemType)).enumValueIndex = (int) itemType;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Item Data:", EditorStyles.largeLabel);
            
            DisplayGeneralInfo(sp); //Show fields that applies to all items
            
            if(itemType is ItemType.Armor or ItemType.Weapon or ItemType.Relic) DisplayEquippableInfo(sp); //Show equippable-specific fields
            
            switch (itemType)
            {
                case ItemType.Weapon:
                    EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.projectiles)));
                    break;
                case ItemType.Tower:
                    DisplaySpawnableInfo(sp); //Show spawnable-specific fields
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayGeneralInfo(SerializedProperty sp)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.uiSprite)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemName)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemDescription)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemRarity)));
        }

        private void DisplayEquippableInfo(SerializedProperty sp)
        {
            EditorGUILayout.LabelField("Equippable Item Data:", EditorStyles.largeLabel);
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.statBonuses)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemEffectIds)));
        }

        private void DisplaySpawnableInfo(SerializedProperty sp)
        {
            EditorGUILayout.LabelField("Spawnable Item Data:", EditorStyles.largeLabel);
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.spawnablePrefab)));
        }
    }
}
