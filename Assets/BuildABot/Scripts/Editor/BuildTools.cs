using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    public static class BuildTools
    {
        [MenuItem("Build-A-Bot/Build/Create Demo Build")]
        public static void CreateDemoBuild()
        {
            string path = EditorUtility.SaveFolderPanel("Choose Location of Demo", 
                EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.selectedStandaloneTarget),
                "");
            
            int sceneCount = EditorBuildSettings.scenes.Length;
            string[] scenes = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                var s = EditorBuildSettings.scenes[i];
                scenes[i] = s.path;
                Debug.Log($"Scene {i}: {s.path}");
            }
            
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                $"{defines}{(string.IsNullOrEmpty(defines) ? "" : ";")}DEMO_BUILD");

            Debug.Log($"Creating demo build {path}/{Application.productName}-Demo.exe");
            
            BuildPipeline.BuildPlayer(scenes, $"{path}/{Application.productName}-Demo.exe",
                EditorUserBuildSettings.selectedStandaloneTarget, BuildOptions.None);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                defines);

        }
        
        [MenuItem("Build-A-Bot/Build/Create Release Build")]
        public static void CreateReleaseBuild()
        {
            string path = EditorUtility.SaveFolderPanel("Choose Location of Build", 
                EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.selectedStandaloneTarget),
                "");
            
            int sceneCount = EditorBuildSettings.scenes.Length;
            string[] scenes = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                var s = EditorBuildSettings.scenes[i];
                scenes[i] = s.path;
                Debug.Log($"Scene {i}: {s.path}");
            }
            
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                $"{defines}{(string.IsNullOrEmpty(defines) ? "" : ";")}RELEASE_BUILD");

            Debug.Log($"Creating release build {path}/{Application.productName}.exe");
            
            BuildPipeline.BuildPlayer(scenes, $"{path}/{Application.productName}.exe",
                EditorUserBuildSettings.selectedStandaloneTarget, BuildOptions.None);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                defines);

        }
    }
}