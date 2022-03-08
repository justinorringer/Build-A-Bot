using System;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    /**
     * A component that allows an object to listen for gameplay events while automatically handling subscription to
     * and unsubscription from the event.
     */
    public class GameplayEventListener : MonoBehaviour
    {
        [SerializeField] private GameplayEvent gameplayEvent;
        [SerializeField] private UnityEvent onEventReceived;

        /** The gameplay event handled by this listener. */
        public GameplayEvent GameplayEvent => gameplayEvent;
        
        protected void OnEnable()
        {
            gameplayEvent.Register(this, onEventReceived.Invoke);
        }

        protected void OnDisable()
        {
            gameplayEvent.Unregister(this);
        }

        /**
         * Adds an action to this listener that will be called whenever this listener is notified about an event.
         * <param name="action">The action that will occur when the event is fired.</param>
         * <remarks>To remove an added action use the GameplayEventListener.RemoveAction function.</remarks>
         */
        public void AddAction(UnityAction action)
        {
            onEventReceived.AddListener(action);
        }

        /**
         * Removes the provided action from the list of actions that will fire when an event is received.
         * <param name="action">The action to remove from the dispatcher.</param>
         */
        public void RemoveAction(UnityAction action)
        {
            onEventReceived.RemoveListener(action);
        }
    }
}