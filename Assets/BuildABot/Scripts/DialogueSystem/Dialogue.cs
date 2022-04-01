using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{

    // TODO: Add cutom property drawers to clean up display
    
    /**
     * A single response option presented by a dialogue frame.
     */
    [Serializable]
    public struct DialogueResponse
    {
        [Tooltip("The text displayed for this response.")]
        [SerializeField] private string text;

        [Tooltip("The index of the dialogue node to go to if this response is chosen.")]
        [Min(0)]
        [SerializeField] private int targetNode;
        
        [Tooltip("The set of gameplay events triggered if this response is chosen.")]
        [SerializeField] private List<GameplayEvent> selectionEvents;

        /** The text displayed for this response. */
        public string Text => text;

        /** The index of the dialogue node to go to if this response is chosen. */
        public int TargetNode => targetNode;

        /** The set of gameplay events triggered if this response is chosen. */
        public ReadOnlyCollection<GameplayEvent> SelectionEvents => selectionEvents.AsReadOnly();
    }
    
    /**
     * A single node of dialogue. A node of dialogue is dialogue content followed by 
     * the end of the conversation or a set of options.
     */
    [Serializable]
    public class DialogueNode
    {
        [Tooltip("The dialogue content stated by this node.")]
        [TextArea(3, 10)]
        [SerializeField] private string content; // TODO: Convert back to list

        [Tooltip("The node that will be played after this node assuming that no responses are used. A value of -1 should end the conversation.")]
        [Min(-1)]
        [SerializeField] private int nextNode; // TODO: Custom property drawer to hide this if responseOptions is > 0

        [Tooltip("The response options presented to the player at the end of this frame.")]
        [SerializeField] private List<DialogueResponse> responseOptions = new List<DialogueResponse>();

        [Tooltip("The expression override index to use when displaying this node. A value of -1 will use the default.")]
        [Min(-1)]
        [SerializeField] private int expressionOverride = -1;

        [Tooltip("The set of gameplay events triggered by this dialogue frame being completed.")]
        [SerializeField] private List<GameplayEvent> completionEvents = new List<GameplayEvent>();

        /** The dialogue content stated by this node. */
        public string Content => content;

        /** The node that will be played after this node assuming that no responses are used. A value of -1 should end the conversation. */
        public int NextNode => nextNode;

        /** The response options presented to the player at the end of this frame. */
        public ReadOnlyCollection<DialogueResponse> ResponseOptions => responseOptions.AsReadOnly();

        /** The expression override index to use when displaying this node. A value of -1 will use the default. */
        public int ExpressionOverride => expressionOverride;

        /** The set of gameplay events triggered by this dialogue frame being completed. */
        public ReadOnlyCollection<GameplayEvent> CompletionEvents => completionEvents.AsReadOnly();
    }
    
    /**
     * An object representing a tree of dialogue that can be spoken to the player by an NPC.
     */
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Build-A-Bot/Dialogue/Dialogue", order = 1)]
    public class Dialogue : ScriptableObject
    {

        [Tooltip("The collection of dialogue nodes that make up this dialogue tree. The first node will always be the entry point.")]
        [SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();

        /** The collection of dialogue nodes that make up this dialogue tree. The first node will always be the entry point. */
        public ReadOnlyCollection<DialogueNode> DialogueNodes => dialogueNodes.AsReadOnly();
    }
}