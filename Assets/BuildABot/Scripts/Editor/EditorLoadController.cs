using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    [InitializeOnLoad]
    public static class EditorLoadController
    {
        static EditorLoadController()
        {
            //EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/BuildABot/Scenes/PersistentGame.unity");
            EditorApplication.playModeStateChanged += ForceLoadPersistentScene;
        }

        private static void ForceLoadPersistentScene(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && !SceneManager.GetSceneByPath("Assets/BuildABot/Scenes/PersistentGame.unity").isLoaded)
            {
                string[] scenes = new string[SceneManager.sceneCount];
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    scenes[i] = SceneManager.GetSceneAt(i).name;
                }
                AsyncOperation loadTask = SceneManager.LoadSceneAsync("PersistentGame", LoadSceneMode.Single);
                loadTask.completed += operation =>
                {
                    foreach (string scene in scenes)
                    {
                        Debug.Log($"Loaded scene {scene}");
                        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                    }
                };
            }
        }
    }
}