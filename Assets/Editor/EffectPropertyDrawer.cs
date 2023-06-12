using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TonyDev.Game.Core.Effects;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;

namespace TonyDev.Editor
{
    [CustomPropertyDrawer(typeof(GameEffectList), true)]
    public class EffectPropertyDrawer : PropertyDrawer
    {
        private GUIStyle _richTextStyle = new GUIStyle
        {
            richText = true
        };


        public object GetValue(SerializedProperty prop)
        {
            string path = prop.propertyPath;
            object obj = prop.serializedObject.targetObject;

            return GetValue(obj, path);
        }
        
        public object GetValue(object source, string name)
        {
            if(source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }
        
        public object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while(index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
        
        private bool _recording;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var value = (GameEffectList) GetValue(property);

            List<GameEffect> itemEffects = value?.gameEffects;//effectsList.FindPropertyRelative("gameEffects").;

            if (itemEffects == null)
            {
                //Debug.LogWarning("Something went wrong with the effect displayer!");
                return;
            }

            itemEffects.RemoveAll(ie => ie == null);

            //EditorGUI.BeginProperty(position, label, property);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("<color=white><size=15><b>Effects</b></size></color>", _richTextStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("New", EditorStyles.miniButton))
            {
                CreateEffectWindow.ShowWindow();
            }

            if (GUILayout.Button("Add", EditorStyles.miniButton))
            {
                SearchWindow.Open(
                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    new EffectListSearchProvider(Game.Global.GameTools.GetTypes<GameEffect>().ToArray(),
                        (x) => AddEffect(itemEffects, x)));
            }

            EditorGUILayout.EndHorizontal();

            var style = new GUIStyle
            {
                richText = true
            };

            EditorGUILayout.Space();

            var itemEffectsProp = property.FindPropertyRelative("gameEffects");

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
                    "<size=12><b><color=white>" + ge.ToString().Split('.').LastOrDefault()?.Replace("Entry", "") +
                    "</color></b></size>",
                    style);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Del", EditorStyles.miniButton))
                {
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    value.gameEffects.Remove(ge);
                    property.serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndHorizontal();

                var effectProp = itemEffectsProp.GetArrayElementAtIndex(i);

                foreach (var field in ge.GetType().GetFields().Where(f =>
                    !f.IsNotSerialized && f.IsPublic && !f.GetCustomAttributes(typeof(HideInInspector)).Any()))
                {
                    //Any changes here should be reflected in CustomReadWrite as well
                    if (field.FieldType == typeof(KeyCode))
                    {
                        EditorGUILayout.BeginHorizontal();

                        var index = (int) field.GetValue(ge);

                        var e = (Enum) Enum.ToObject(field.FieldType, index);

                        field.SetValue(ge, Enum.ToObject(field.FieldType, EditorGUILayout.EnumPopup(field.Name, e)));

                        if (GUILayout.Button(_recording ? "..." : "Record", EditorStyles.miniButton))
                        {
                            _recording = true;
                        }

                        if (_recording)
                        {
                            var k = CheckForKey();
                            if (k != KeyCode.None)
                            {
                                field.SetValue(ge, k);
                                _recording = false;
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

                    /*switch (Type.GetTypeCode(field.FieldType))
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
                    }*/

                    var prop = effectProp.FindPropertyRelative(field.Name);
                    EditorGUILayout.PropertyField(prop, new GUIContent(field.Name), true);

                    /*if (field.FieldType == typeof(Sprite))
                    {
                        field.SetValue(ge, EditorGUILayout.ObjectField(field.Name, (Sprite) field.GetValue(ge), typeof(Sprite), false));
                        continue;
                    }*/

                    /*if (field.FieldType == typeof(ProjectileData))
                    {

                        Debug.Log(projProperty.type);

                        EditorGUILayout.PropertyField(projProperty, new GUIContent(field.Name), true);
                        //How?
                    }*/
                }

                EditorGUILayout.Space();

                //EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                //EditorGUI.EndProperty();
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
        
        private void AddEffect(List<GameEffect> list, Type t)
        {
            var ge = (GameEffect) Activator.CreateInstance(t);

            list.Add(ge);
        }
    }
}