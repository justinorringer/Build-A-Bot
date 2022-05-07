using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    public class LoadSceneControl : MonoBehaviour, IMenuControl
    {
        [Tooltip("The name of the scene to load.")]
        [SerializeField] private string scene; // TODO: Create or use a scene selection tool

        [Tooltip("Should the spawning of a new player be suppressed when performing this load?")]
        [SerializeField] private bool suppressPlayerSpawn;
        
        public void Execute()
        {
            //SceneManager.LoadScene(scene, LoadSceneMode.Single);
            GameManager.OpenLevel(scene, LoadSceneMode.Single, suppressPlayerSpawn);
        }
    }
}