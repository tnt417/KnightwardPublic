using TonyDev.Game.Level.Rooms.ProceduralGen;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Editor
{
    [CustomEditor(typeof(ProceduralRoomGenerator))]
    public class ProceduralRoomGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ProceduralRoomGenerator bounds = (ProceduralRoomGenerator)target;

            // Draw the default inspector with the min/max sliders
            DrawDefaultInspector();

            // Add a space and a button to regenerate
            EditorGUILayout.Space();
            if (GUILayout.Button("Regenerate Room"))
            {
                GenerateTestRoom(bounds);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateTestRoom(ProceduralRoomGenerator generator)
        {
            // Call your generation function
            generator.Regen();

            Debug.Log("Generated test room with new weights.");
        }
    }
}