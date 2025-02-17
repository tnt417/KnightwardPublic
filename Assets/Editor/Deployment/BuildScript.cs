using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TonyDev.Editor.Deployment
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
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Waiting for scripts to compile...");
                System.Threading.Thread.Sleep(500);
            }
            
            // Define output paths
            string buildFolder = isDemo ? "Builds/Demo" : "Builds/Full";
            string winPath = Path.Combine(buildFolder, "Windows", "Knightward.exe");
            string macPath = Path.Combine(buildFolder, "MacOS", "Knightward");

            // Get active scenes
            string[] scenes =
            {
                "Assets/Scenes/MainMenuScene.unity",
                "Assets/Scenes/LobbyScene.unity",
                "Assets/Scenes/CastleScene.unity",
                "Assets/Scenes/GameOver.unity"
            };

            // Ensure the directory exists
            Directory.CreateDirectory(buildFolder);
            
            Directory.CreateDirectory(buildFolder + "/Windows");
            Directory.CreateDirectory(buildFolder + "/MacOS");

            // Set define symbols for demo or full builds
            string defineSymbols = isDemo ? "IS_DEMO" : "";  
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defineSymbols);

            Debug.Log("Building Windows at: " + winPath);
            
            // Build for Windows
            BuildPipeline.BuildPlayer(scenes, winPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log(isDemo ? "Demo Windows Build Complete!" : "Full Windows Build Complete!");

            Debug.Log("Building MacOS at: " + macPath);
            
            // Build for Mac
            BuildPipeline.BuildPlayer(scenes, macPath, BuildTarget.StandaloneOSX, BuildOptions.None);
            Debug.Log(isDemo ? "Demo MacOS Build Complete!" : "Full MacOS Build Complete!");
        }
    }
}

