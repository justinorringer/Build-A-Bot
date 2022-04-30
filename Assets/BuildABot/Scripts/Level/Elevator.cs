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
            base.OnInteract(instigator);
        }
    }
}