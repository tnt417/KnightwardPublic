using TonyDev.Game.Level.Rooms.ProceduralGen;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenWeightsBounds))]
public class GenWeightBoundsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GenWeightsBounds bounds = (GenWeightsBounds)target;

        // Alternate background color toggle
        bool isDark = false;

        DrawMinMaxSlider(ref bounds.roomDifficulty, "Room Difficulty", ref isDark);
        DrawMinMaxSlider(ref bounds.roomTreasureLevel, "Room Treasure Level", ref isDark);
        DrawMinMaxSlider(ref bounds.roomArea, "Room Area", ref isDark);
        DrawMinMaxSlider(ref bounds.roomOpenness, "Room Openness", ref isDark);
        DrawMinMaxSlider(ref bounds.roomHorizontalBias, "Room Horizontal Bias", ref isDark);
        DrawMinMaxSlider(ref bounds.roomVerticalBias, "Room Vertical Bias", ref isDark);
        DrawMinMaxSlider(ref bounds.roomSprawl, "Room Sprawl", ref isDark);
        DrawMinMaxSlider(ref bounds.roomMoisture, "Room Moisture", ref isDark);
        DrawMinMaxSlider(ref bounds.roomVegetation, "Room Vegetation", ref isDark);
        DrawMinMaxSlider(ref bounds.roomDilapidatedness, "Room Dilapidatedness", ref isDark);
        DrawMinMaxSlider(ref bounds.pathGenChance, "Path Generation Chance", ref isDark);
        DrawMinMaxSlider(ref bounds.roomRubbleAmount, "Room Rubble Amount", ref isDark);
        DrawMinMaxSlider(ref bounds.roomRigidity, "Room Rigidity", ref isDark);
        DrawMinMaxSlider(ref bounds.roomCoziness, "Room Coziness", ref isDark);
        DrawMinMaxSlider(ref bounds.roomBrightnessLevel, "Room Brightness Level", ref isDark);
        DrawMinMaxSlider(ref bounds.roomTemperature, "Room Temperature", ref isDark);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawMinMaxSlider(ref Vector2 range, string label, ref bool isDark)
    {
        GUIStyle boldStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };

        // Alternate background colors
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = isDark ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.95f, 0.95f, 0.95f);
        isDark = !isDark; // Toggle for next field

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, boldStyle);

        range.x = Mathf.Clamp(range.x, 0f, 1f);
        range.y = Mathf.Clamp(range.y, 0f, 1f);

        // MinMax slider
        EditorGUILayout.MinMaxSlider(ref range.x, ref range.y, 0f, 1f);

        // Display Min and Max floats on the same line
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min", GUILayout.Width(30));
        range.x = EditorGUILayout.FloatField(range.x, GUILayout.Width(50));
        EditorGUILayout.LabelField("Max", GUILayout.Width(30));
        range.y = EditorGUILayout.FloatField(range.y, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = originalColor;
    }
}
