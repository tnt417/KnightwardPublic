using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Items;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Editor
{
    
    public class CreateEffectWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            GetWindow(typeof(CreateEffectWindow));
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Create Effect");
            maxSize = new Vector2(300f, 110f);
            minSize = maxSize;
        }

        public ItemData attachItem;
        private string effectName;
        private bool edit;
        private string EffectClassName => effectName.Replace(" ", "");

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 100f;

            EditorGUILayout.LabelField("Effect Creation", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //Effect Name

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Effect Name", EditorStyles.boldLabel);

            var a = EditorGUILayout.TextField(effectName);

            effectName = string.IsNullOrEmpty(a) ? "" : new string(a.Where(char.IsLetter).ToArray());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Width(10f);

            edit = EditorGUILayout.Toggle("Open", edit);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create"))
            {
                Create();
                if (edit)
                    AssetDatabase.OpenAsset(
                        AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Game/Core/Effects/" + EffectClassName +
                                                                  ".cs"));
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        public void Create()
        {
            var template = AssetDatabase.LoadAssetAtPath("Assets/EffectTemplate.txt",
                typeof(TextAsset)) as TextAsset;

            var contents = "";

            if (template != null)
            {
                contents = template.text;
                contents = contents.Replace("EFFECT_NAME", EffectClassName);
            }
            else
            {
                Debug.LogError("Can't find EffectTemplate.txt");
            }

            using (var sw = new StreamWriter(string.Format(Application.dataPath + "/Game/Core/Effects/{0}.cs",
                new object[] {EffectClassName})))
            {
                sw.Write(contents);
            }

            AssetDatabase.Refresh();
        }
    }
}