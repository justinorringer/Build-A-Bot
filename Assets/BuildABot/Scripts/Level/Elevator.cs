using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace BuildABot
{
    public class Elevator : InteractableObject
    {

        [SerializeField] private string sceneToLoad;

        /** The current gamer. */
        public Player Gamer { get; private set; }

        protected override void OnInteract(InteractionController instigator)
        {
            Debug.LogFormat("Elevator: {0}", sceneToLoad);
            Gamer = instigator.Player;
            GoingUp();
            base.OnInteract(instigator);
        }

        private void GoingUp()
        {
            Debug.LogFormat("In ButtonPress");

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