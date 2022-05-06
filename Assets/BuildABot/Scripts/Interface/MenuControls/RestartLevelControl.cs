using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    public class RestartLevelControl : MonoBehaviour, IMenuControl
    {
        [Tooltip("The name of the scene to load.")]
        [SerializeField] private string scene; // TODO: Create or use a scene selection tool
        
        [Tooltip("The name of the scene to load when in the tutorial level.")]
        [SerializeField] private string tutorialTargetScene;
        
        public void Execute()
        {
            if (!GameManager.Initialized) GameManager.OpenLevel(scene);
            else
            {
                if (GameManager.GameState.CompletedLevelCount == 0) GameManager.OpenLevel(tutorialTargetScene);
                else GameManager.OpenLevel(scene);
            }
        }
    }
}