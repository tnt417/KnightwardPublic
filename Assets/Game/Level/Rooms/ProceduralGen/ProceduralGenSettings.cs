using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms.ProceduralGen
{

    [Serializable]
    public struct GenWeights
    {
        [Range(0f, 1f)] public float roomDifficulty;
    [Range(0f, 1f)] public float roomTreasureLevel;
    [Range(0f, 1f)] public float roomArea;
    [Range(0f, 1f)] public float roomOpenness;
    [Range(0f, 1f)] public float roomHorizontalBias;
    [Range(0f, 1f)] public float roomVerticalBias;
    [Range(0f, 1f)] public float roomSprawl;
    [Range(0f, 1f)] public float roomMoisture;
    [Range(0f, 1f)] public float roomVegetation;
    [Range(0f, 1f)] public float roomDilapidatedness;
    [Range(0f, 1f)] public float pathGenChance;
    [Range(0f, 1f)] public float roomRubbleAmount;
    [Range(0f, 1f)] public float roomRigidity;
    [Range(0f, 1f)] public float roomCoziness;
    [Range(0f, 1f)] public float roomBrightnessLevel;
    [Range(0f, 1f)] public float roomTemperature;

    // Constructor for easy initialization
    public GenWeights(
        float difficulty, float treasure, float area, float openness,
        float hBias, float vBias, float sprawl, float moisture, float vegetation,
        float dilapidatedness, float pathChance, float rubble, float rigidity,
        float coziness, float brightness, float temperature)
    {
        roomDifficulty = difficulty;
        roomTreasureLevel = treasure;
        roomArea = area;
        roomOpenness = openness;
        roomHorizontalBias = hBias;
        roomVerticalBias = vBias;
        roomSprawl = sprawl;
        roomMoisture = moisture;
        roomVegetation = vegetation;
        roomDilapidatedness = dilapidatedness;
        pathGenChance = pathChance;
        roomRubbleAmount = rubble;
        roomRigidity = rigidity;
        roomCoziness = coziness;
        roomBrightnessLevel = brightness;
        roomTemperature = temperature;
    }

    // Generate randomized weights within a given range
    public static GenWeights GenerateRandom(GenWeightsBounds bounds)
    {
        return new GenWeights(
            Random.Range(bounds.roomDifficulty.x, bounds.roomDifficulty.y),
            Random.Range(bounds.roomTreasureLevel.x, bounds.roomTreasureLevel.y),
            Random.Range(bounds.roomArea.x, bounds.roomArea.y),
            Random.Range(bounds.roomOpenness.x, bounds.roomOpenness.y),
            Random.Range(bounds.roomHorizontalBias.x, bounds.roomHorizontalBias.y),
            Random.Range(bounds.roomVerticalBias.x, bounds.roomVerticalBias.y),
            Random.Range(bounds.roomSprawl.x, bounds.roomSprawl.y),
            Random.Range(bounds.roomMoisture.x, bounds.roomMoisture.y),
            Random.Range(bounds.roomVegetation.x, bounds.roomVegetation.y),
            Random.Range(bounds.roomDilapidatedness.x, bounds.roomDilapidatedness.y),
            Random.Range(bounds.pathGenChance.x, bounds.pathGenChance.y),
            Random.Range(bounds.roomRubbleAmount.x, bounds.roomRubbleAmount.y),
            Random.Range(bounds.roomRigidity.x, bounds.roomRigidity.y),
            Random.Range(bounds.roomCoziness.x, bounds.roomCoziness.y),
            Random.Range(bounds.roomBrightnessLevel.x, bounds.roomBrightnessLevel.y),
            Random.Range(bounds.roomTemperature.x, bounds.roomTemperature.y)
        );
    }
    }

    [CreateAssetMenu(fileName = "GenSettings", menuName = "ProceduralGen/Weights Bounds")]
    public class GenWeightsBounds : ScriptableObject
    {
        public Vector2 roomDifficulty;
        public Vector2 roomTreasureLevel;
        public Vector2 roomArea;
        public Vector2 roomOpenness;
        public Vector2 roomHorizontalBias;
        public Vector2 roomVerticalBias;
        public Vector2 roomSprawl;
        public Vector2 roomMoisture;
        public Vector2 roomVegetation;
        public Vector2 roomDilapidatedness;
        public Vector2 pathGenChance;
        public Vector2 roomRubbleAmount;
        public Vector2 roomRigidity;
        public Vector2 roomCoziness;
        public Vector2 roomBrightnessLevel;
        public Vector2 roomTemperature;
    }
    
    [CreateAssetMenu(fileName = "GenSettings", menuName = "ProceduralGen/Gen Settings")]
    public class ProceduralGenSettings : ScriptableObject
    {
        public GenWeightsBounds weightsBounds;
        public TileBase baseFloorTile;
    }
}
