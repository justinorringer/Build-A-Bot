using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace BuildABot
{
    public class Elevator : MonoBehaviour
    {

        [SerializeField] private string sceneToLoad;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                GameManager.GameState.CompletedLevelCount++;
                if (GameManager.GameState.CompletedLevelCount == 3) GameManager.GameState.GameStage++;
                else if (GameManager.GameState.CompletedLevelCount == 5) GameManager.GameState.GameStage++;
                else if (GameManager.GameState.CompletedLevelCount == 7) GameManager.GameState.GameStage++;
                GameManager.GameState.NextLevelType = GameManager.GameState.GameStage >= 3
                    ? Random.Range(0, 3)
                    : GameManager.GameState.GameStage;
                GameManager.OpenLevel(sceneToLoad);
            }
        }
    }
}