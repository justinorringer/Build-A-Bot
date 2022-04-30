using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildABot
{
    public class ElevatorInteract : MonoBehaviour
    {
        [Tooltip("The elevator with listener.")]
        [SerializeField] private Elevator elevator;

        [Tooltip("The text object used to display the control tips.")]
        [SerializeField] private TMP_Text controlTipText;

        /** The input state cache to restore to when this menu is closed. */
        private PlayerController.InputActionsStateCache _inputStateCache;
        
        public Elevator Elevator => elevator;
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
            //SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
       }
    }
}