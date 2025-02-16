using System.IO;
using UnityEditor;
using UnityEngine;

namespace TonyDev.Deployment
{
    public class BuildScript
    {
        [MenuItem("Custom Build/Build All (Demo)")]
        public static void BuildAllDemo()
        {
            BuildAll(true);
        }

        [MenuItem("Custom Build/Build All (Full)")]
        public static void BuildAllFull()
        {
            BuildAll(false);
        }

        private static void BuildAll(bool isDemo)
        {
            // Define output paths
            string buildFolder = isDemo ? "Builds/Demo" : "Builds/Full";
            string winPath = Path.Combine(buildFolder, "Windows", "Game.exe");
            string macPath = Path.Combine(buildFolder, "MacOS");

            // Get active scenes
            string[] scenes = new string[]
            {
                "Assets/Scenes/MainScene.unity"
            };

            // Ensure the directory exists
            Directory.CreateDirectory(buildFolder);

            // Set define symbols for demo or full builds
            string defineSymbols = isDemo ? "IS_DEMO" : "";  
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defineSymbols);

            // Build for Windows
            BuildPipeline.BuildPlayer(scenes, winPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log(isDemo ? "Demo Windows Build Complete!" : "Full Windows Build Complete!");

            // Build for Mac
            BuildPipeline.BuildPlayer(scenes, macPath, BuildTarget.StandaloneOSX, BuildOptions.None);
            Debug.Log(isDemo ? "Demo MacOS Build Complete!" : "Full MacOS Build Complete!");
        }
    }
}

