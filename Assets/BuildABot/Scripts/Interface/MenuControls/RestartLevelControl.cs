using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    public class RestartLevelControl : MonoBehaviour, IMenuControl
    {
        [Tooltip("The name of the scene to load.")]
        [SerializeField] private string scene; // TODO: Create or use a scene selection tool
        
        [Tooltip("The position in the scene to put the player at in the tutorial level.")]
        [SerializeField] private Vector3 tutorialStartPosition;
        
        public void Execute()
        {
            if (!GameManager.Initialized) GameManager.OpenLevel(scene);
            else
            {
                if (GameManager.GameState.CompletedLevelCount == 0) GameManager.GetPlayer().transform.position = tutorialStartPosition;
                else GameManager.OpenLevel(scene);
            }
        }
    }
}