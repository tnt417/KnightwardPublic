using UnityEditor;
using UnityEngine;

public class SpriteSlicer : EditorWindow
{
    [MenuItem("Tools/Copy Sprite Slicing Settings")]
    static void CopySlicingSettings()
    {
        TextureImporter source = (TextureImporter)AssetImporter.GetAtPath("Assets/Game/Level/Tiles/Walls/castleWallThin.png");
        TextureImporter target = (TextureImporter)AssetImporter.GetAtPath("Assets/Game/Level/Tiles/Walls/castleWallFire.png");

        if (source != null && target != null)
        {
            target.spriteImportMode = source.spriteImportMode;
            target.spritePixelsPerUnit = source.spritePixelsPerUnit;
            target.spriteBorder = source.spriteBorder;

            if (source.spriteImportMode == SpriteImportMode.Multiple)
            {
                target.spritesheet = source.spritesheet;
            }

            AssetDatabase.ImportAsset("Assets/PathToYourTargetSprite.png", ImportAssetOptions.ForceUpdate);
            Debug.Log("Slicing settings copied successfully.");
        }
        else
        {
            Debug.LogError("Source or target sprite not found.");
        }
    }
}