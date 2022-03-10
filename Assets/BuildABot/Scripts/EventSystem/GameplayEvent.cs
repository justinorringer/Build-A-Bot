using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * A globally referencable gameplay event that can be triggered by anyone and received by arbitrary handlers.
     * The benefit of this system is that any object can become a listener for a gameplay event without needing
     * a central location where listeners are listed out manually like with UnityEvent dispatchers.
     */
    [CreateAssetMenu(fileName = "NewGameplayEvent", menuName = "Build-A-Bot/Gameplay Event", order = 2)]
    public class GameplayEvent : ScriptableObject
    {
        /** The set of subscribed listeners kept at runtime. */
        private readonly Dictionary<GameplayEventListener, Action> _listeners = new Dictionary<GameplayEventListener, Action>();

        /**
         * Registers the provided listener to this event. This will trigger the listener whenever this event is invoked.
         * <remarks>Listeners will automatically call this function and as such should not be manually called.</remarks>
         * <param name="listener">The listener to subscribe.</param>
         * <param name="onInvoke">The action that will fire when this event is invoked.</param>
         */
        public void Register(GameplayEventListener listener, Action onInvoke)
        {
            _listeners.Add(listener, onInvoke);
        }
        
        /**
         * Unregisters the provided listener from this event. This will remove the listener from receiving
         * notifications about this event.
         * <remarks>Listeners will automatically call this function and as such should not be manually called.</remarks>
         * <param name="listener">The listener to subscribe.</param>
         */
        public void Unregister(GameplayEventListener listener)
        {
            _listeners.Remove(listener);
        }

        /**
         * Invokes this event notifying all registered listeners.
         */
        public void Invoke()
        {
            foreach (var entry in _listeners)
            {
                entry.Value.Invoke();
            }
        }
    }
}