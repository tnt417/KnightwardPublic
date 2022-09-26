using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.SEIDInfo;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Items;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TonyDev.Editor
{
    [CustomEditor(typeof(ItemData))]
    public class ItemEditor : UnityEditor.Editor
    {
        public ItemType itemType;

        public List<GameEffect> itemEffects = new();

        private GUIStyle richTextStyle;

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[]
                guids = AssetDatabase.FindAssets("t:" +
                                                 typeof(T)
                                                     .Name); //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++) //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        private ItemData otherItem;

        private void OnEnable()
        {
            if (serializedObject.targetObject is ItemData id)
            {
                itemEffects = id.item.itemEffects;
            }

            richTextStyle = new GUIStyle
            {
                richText = true
            };
        }

        // The function that makes the custom editor work
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(serializedObject.targetObject);
            
            GUILayout.Label("<color=white><size=15><b>Item Manager</b></size></color>", richTextStyle);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Browse"))
            {
                SearchWindow.Open(
                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    new ItemListSearchProvider(GetAllInstances<ItemData>(), (x) => { otherItem = x; }));
            }

            if (GUILayout.Button("Create"))
            {
                CreateItemWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();

            if (otherItem != null) AssetDatabase.OpenAsset(otherItem);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("<color=white><size=15><b>Item</b></size></color>", richTextStyle);

            serializedObject.UpdateIfRequiredOrScript();

            var sp = serializedObject.FindProperty("item");
            itemType = (ItemType) EditorGUILayout.EnumPopup("Type",
                (ItemType) sp.FindPropertyRelative(nameof(Item.itemType)).enumValueIndex);

            sp.FindPropertyRelative(nameof(Item.itemType)).enumValueIndex = (int) itemType;

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("<color=white><size=15><b>General</b></size></color>", richTextStyle);

            EditorGUILayout.Space();

            DisplayGeneralInfo(sp); //Show fields that applies to all items

            if (itemType is ItemType.Armor or ItemType.Weapon or ItemType.Relic)
                DisplayEquippableInfo(sp); //Show equippable-specific fields

            if (itemType is ItemType.Tower) DisplaySpawnableInfo(sp); //Show spawnable-specific fields

            if (serializedObject.targetObject is ItemData id) id.item.itemEffects = itemEffects;
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayGeneralInfo(SerializedProperty sp)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemName)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemRarity)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.uiSprite)));
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.itemDescription)));
        }

        private void DisplayEquippableInfo(SerializedProperty sp)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("<color=white><size=15><b>Additional</b></size></color>", richTextStyle);

            EditorGUILayout.Space();

            if (itemType == ItemType.Weapon)
                EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.projectiles)));

            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.statBonuses)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DisplayEffectInfo(sp);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private bool recording = false;
        
        private void DisplayEffectInfo(SerializedProperty sp)
        {
            itemEffects.RemoveAll(ie => ie == null);
            
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("<color=white><size=15><b>Effects</b></size></color>", richTextStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("New", EditorStyles.miniButton))
            {
                CreateEffectWindow.ShowWindow();
            }

            if (GUILayout.Button("Add", EditorStyles.miniButton))
            {
                SearchWindow.Open(
                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    new EffectListSearchProvider(Game.Global.Tools.GetTypes<GameEffect>().ToArray(),
                        (x) => AddEffect(x)));
            }

            EditorGUILayout.EndHorizontal();

            var style = new GUIStyle
            {
                richText = true
            };

            EditorGUILayout.Space();

            for (var i = 0; i < itemEffects.Count; i++)
            {
                var screenRect = GUILayoutUtility.GetRect(1, 1);
                var vertRect = EditorGUILayout.BeginVertical();

                EditorGUI.DrawRect(
                    new Rect(screenRect.x - 13, screenRect.y - 1, screenRect.width + 17, vertRect.height + 9),
                    EditorStyleTools.GetPastelRainbow(i));

                var ge = itemEffects[i];

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(
                    "<size=12><b><color=white>" + ge.ToString().Split('.').LastOrDefault() + "</color></b></size>",
                    style);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Del", EditorStyles.miniButton))
                {
                    itemEffects.Remove(ge);
                }

                EditorGUILayout.EndHorizontal();

                foreach (var field in ge.GetType().GetFields().Where(f => !f.IsNotSerialized && f.IsPublic))
                { //Any changes here should be reflected in CustomReadWrite as well
                    if (field.FieldType.IsEnum)
                    {
                        EditorGUILayout.BeginHorizontal();

                        var index = (int) field.GetValue(ge);
                        
                        var e = (Enum) Enum.ToObject(field.FieldType, index);
                        
                        field.SetValue(ge,Enum.ToObject(field.FieldType, EditorGUILayout.EnumPopup(field.Name, e)));

                        if (field.FieldType == typeof(KeyCode))
                        {
                            if (GUILayout.Button(recording ? "..." : "Record", EditorStyles.miniButton))
                            {
                                recording = true;
                            }

                            if (recording)
                            {
                                var k = CheckForKey();
                                if (k != KeyCode.None)
                                {
                                    field.SetValue(ge, k);
                                    recording = false;
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        continue;
                    }
                    
                    switch (Type.GetTypeCode(field.FieldType))
                    {
                        case TypeCode.Int32:
                            field.SetValue(ge, EditorGUILayout.IntField(field.Name, (int) field.GetValue(ge)));
                            continue;
                        case TypeCode.Boolean:
                            field.SetValue(ge, EditorGUILayout.Toggle(field.Name, (bool) field.GetValue(ge)));
                            continue;
                        case TypeCode.String:
                            field.SetValue(ge, EditorGUILayout.TextField(field.Name, (string) field.GetValue(ge)));
                            continue;
                        case TypeCode.Double:
                            field.SetValue(ge, EditorGUILayout.DoubleField(field.Name, (double) field.GetValue(ge)));
                            continue;
                        case TypeCode.Single:
                            field.SetValue(ge, EditorGUILayout.FloatField(field.Name, (float) field.GetValue(ge)));
                            continue;
                        default:
                            break;
                    }

                    if (field.FieldType == typeof(Sprite))
                    {
                        field.SetValue(ge, EditorGUILayout.ObjectField(field.Name, (Sprite) field.GetValue(ge), typeof(Sprite), false));
                        continue;
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }
        }

        private KeyCode CheckForKey()
        {
            var current = Event.current;
            
            return current.type switch
            {
                EventType.KeyDown => Event.current.keyCode,
                _ => KeyCode.None
            };
        }

        private void AddEffect(Type t)
        {
            if (serializedObject.targetObject is ItemData id)
            {
                var ge = (GameEffect) Activator.CreateInstance(t);
                
                itemEffects.Add(ge);

                if (ge is AbilityEffect ae)
                {
                    ae.abilitySprite = id.item.uiSprite;
                }
            }
        }

        private void DisplaySpawnableInfo(SerializedProperty sp)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("<size=15><b><color=white>Spawnable</color></b></size>", richTextStyle);
            EditorGUILayout.PropertyField(sp.FindPropertyRelative(nameof(Item.spawnablePrefabName)));
        }
    }
}