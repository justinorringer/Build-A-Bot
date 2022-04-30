using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BuildABot
{

    /**
     * An emotional expression that can be used to override the behaviors of the dialogues view when a character is speaking.
     */
    [Serializable]
    public struct DialogueSpeakerExpression
    {

        [Tooltip("The name of this expression.")]
        [SerializeField] private string expressionName;
        
        [Tooltip("The sprite used to override the default character sprite when this expression is used. If left empty, the default will be used.")]
        [SerializeField] private Sprite spriteOverride;
        
        [Tooltip("The audio used to override the default character speaking audio when this expression is used. If left empty, the default will be used.")]
        [SerializeField] private AudioClip soundOverride;

        /** The name of this expression. */
        public string Name => expressionName;

        /** The sprite used to override the default character sprite when this expression is used. If left empty, the default will be used. */
        public Sprite SpriteOverride => spriteOverride;

        /** The audio used to override the default character speaking audio when this expression is used. If left empty, the default will be used. */
        public AudioClip SoundOverride => soundOverride;
    }
    
    /**
     * A profile used to display metadata about a single character when in a dialogue interaction.
     */
    [CreateAssetMenu(fileName = "NewDialogueSpeaker", menuName = "Build-A-Bot/Dialogue/DialogueSpeaker", order = 1)]
    public class DialogueSpeaker : ScriptableObject
    {
        [Tooltip("The name of this speaker displayed when they are speaking to the player.")]
        [SerializeField] private string characterName;

        [Tooltip("The sound played whenever this character's dialogue is printed onto the UI.")]
        [SerializeField] private AudioClip speakingSound;
        
        [Tooltip("The default image used to represent this character in dialogue views.")]
        [SerializeField] private Sprite defaultCharacterSprite;

        [Tooltip("A list of expressions that can be used to override the default sprite and sound used for this character. These can be selected by the dialogue tree.")]
        [SerializeField] private List<DialogueSpeakerExpression> expressions;

        /** The name of this speaker displayed when they are speaking to the player. */
        public string CharacterName => characterName;

        /** The sound played whenever this character's dialogue is printed onto the UI. */
        public AudioClip SpeakingSound => speakingSound;

        /** The default image used to represent this character in dialogue views. */
        public Sprite DefaultCharacterSprite => defaultCharacterSprite;

        /** A list of expressions that can be used to override the default sprite and sound used for this character. These can be selected by the dialogue tree. */
        public ReadOnlyCollection<DialogueSpeakerExpression> Expressions => expressions.AsReadOnly();
    }
}