using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    public class LevelTimeout : MonoBehaviour
    {

        [Tooltip("The time it takes for the scene to exit.")]
        [SerializeField] private float timeoutDuration;
        
        [Tooltip("The name of the scene to load.")]
        [SerializeField] private string scene; // TODO: Create or use a scene selection tool

        protected void Start()
        {
            Utility.DelayedFunction(this, timeoutDuration, Execute, true);
        }

        private void Execute()
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
    }
}