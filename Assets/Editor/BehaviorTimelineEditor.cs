using System;
using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Enemies.Movement;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Editor
{
    [CustomEditor(typeof(BehaviorTimelineData))]
    public class BehaviorTimelineEditor : UnityEditor.Editor
    {
        public List<TimelineEntry> timelineEntries = new();

        private void OnEnable()
        {
            _richTextStyle = new GUIStyle()
            {
                richText = true
            };

            if (serializedObject.targetObject is BehaviorTimelineData btd)
            {
                timelineEntries = btd.timelineEntries;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(serializedObject.targetObject);

            serializedObject.UpdateIfRequiredOrScript();

            Display();

            if (serializedObject.targetObject is BehaviorTimelineData btd) btd.timelineEntries = timelineEntries;
            serializedObject.ApplyModifiedProperties();
        }

        private GUIStyle _richTextStyle;

        private TimelineEntryType _selectedType;

        private void Display()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("<color=white><size=15><b>Timeline</b></size></color>", _richTextStyle);

            _selectedType = (TimelineEntryType) EditorGUILayout.EnumPopup(_selectedType);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add", EditorStyles.miniButton))
            {
                AddEntry(TimelineEntry.EntryTypeDictionary[_selectedType]);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var entriesProp = serializedObject.FindProperty("timelineEntries");

            for (var i = 0; i < timelineEntries.Count; i++)
            {
                var screenRect = GUILayoutUtility.GetRect(1, 1);
                var vertRect = EditorGUILayout.BeginVertical();

                EditorGUI.DrawRect(
                    new Rect(screenRect.x - 13, screenRect.y - 1, screenRect.width + 17, vertRect.height + 9),
                    EditorStyleTools.GetPastelRainbow(i));

                var timelineProp = entriesProp.GetArrayElementAtIndex(i);

                var te = timelineEntries[i];
                
                EditorGUILayout.LabelField("<size=12><b><color=white>" + te.ToString().Split('.').LastOrDefault() + "</color></b></size>", _richTextStyle);

                foreach (var field in te.GetType().GetFields().Where(f => !f.IsNotSerialized && f.IsPublic))
                {
                    var prop = timelineProp.FindPropertyRelative(field.Name);
                    EditorGUILayout.PropertyField(prop, new GUIContent(field.Name), true);
                }

                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }
            
            var sr = GUILayoutUtility.GetRect(1, 1);
            var vr = EditorGUILayout.BeginVertical();

            EditorGUI.DrawRect(new Rect(sr.x - 13, sr.y - 1, sr.width + 17, vr.height + 9), Color.white);
            EditorGUILayout.LabelField("<size=12><b><color=black>Repeat</color></b></size>", _richTextStyle);
            
            EditorGUILayout.EndVertical();
        }

        private void AddEntry(Type t)
        {
            if (serializedObject.targetObject is not BehaviorTimelineData) return;

            var te = (TimelineEntry) Activator.CreateInstance(t);

            timelineEntries.Add(te);
        }
    }
}