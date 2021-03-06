using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * The base representation and data of all character attacks.
     */
    public abstract class AttackData : ScriptableObject
    {
        
        [Tooltip("The effects associated with this attack.")]
        [SerializeField] private List<EffectInstance> effects;
        
        [Tooltip("The name of the animation trigger to activate when using this attack.")]
        [SerializeField] private string animationTriggerName = "Attack";
        
        [Header("Sound Information")]
        
        [Tooltip("The sound to play when this attack is started.")]
        [SerializeField] private AudioClip startSound;
        
        [Tooltip("The sound to play when this attack is progresses its execution. Mainly used for projectiles.")]
        [SerializeField] private AudioClip progressSound;

        [Tooltip("The sound to play when this attack hits an enemy.")]
        [SerializeField] private AudioClip hitSound;

        [Tooltip("The sound to play on a particular frame of this attack. This will need to use an animation event which calls the PlayframeSound function.")]
        [SerializeField] private AudioClip frameSound;

        [Header("Offset")]
        
        [Tooltip("The offset to apply between the character and the start point of the attack.")]
        [SerializeField] private Vector2 offset;

        [Tooltip("The offset of the attack along the X axis over time.")]
        [SerializeField] private AnimationCurve offsetXCurve = AnimationCurve.Constant(0, 1, 1);
        
        [Tooltip("The offset of the attack along the Y axis over time.")]
        [SerializeField] private AnimationCurve offsetYCurve = AnimationCurve.Constant(0, 1, 1);

        [Tooltip("Should the offset be multiplied by the character's look direction?")]
        [SerializeField] private bool offsetInLookDirection = true;
        
        [Header("Size")]
        
        [Tooltip("Is the size of this attack relative to the bounds of the attacking character?")]
        [SerializeField] private bool useRelativeSize = true;
        
        [Tooltip("The size of the attack area.")]
        [SerializeField] private Vector2 size = Vector2.one;
        
        [Header("Flags")]

        [Tooltip("Can this attack hit the attacker who triggered it?")]
        [SerializeField] private bool canHitSelf;

        [Tooltip("Can this attack hit the same target multiple times?")]
        [SerializeField] private bool allowMultiHit;
        
        [Tooltip("Is movement of the character allowed while performing this attack?")]
        [SerializeField] private bool allowMovement = true;
        
        [Tooltip("Can this attack be interrupted?")]
        [SerializeField] private bool canInterrupt = true;

        /** The effects associated with this attack. */
        public List<EffectInstance> Effects => effects;

        /** The name of the animation trigger to activate when using this attack. */
        public string AnimationTriggerName => animationTriggerName;

        /** The sound to play when this attack is started. */
        public AudioClip StartSound => startSound;

        /** The sound to play when this attack is progresses its execution. Mainly used for projectiles. */
        public AudioClip ProgressSound => progressSound;

        /** The sound to play when this attack hits an enemy. */
        public AudioClip HitSound => hitSound;

        /** The sound to play on a particular frame of this attack. This will need to use an animation event which calls the PlayframeSound function. */
        public AudioClip FrameSound => frameSound;

        /** The offset to apply between the character and the start point of the attack. */
        public Vector2 Offset => offset;

        /** The offset of the attack along the X axis over time. */
        public AnimationCurve OffsetXCurve => offsetXCurve;

        /** The offset of the attack along the Y axis over time. */
        public AnimationCurve OffsetYCurve => offsetYCurve;

        /** Should the offset be multiplied by the character's look direction? */
        public bool OffsetInLookDirection => offsetInLookDirection;

        /** Is the size of this attack relative to the bounds of the attacking character? */
        public bool UseRelativeSize => useRelativeSize;

        /** The size of the attack area. */
        public Vector2 Size => size;

        /** Can this attack hit the attacker who triggered it? */
        public bool CanHitSelf => canHitSelf;

        /** Can this attack hit the same target multiple times? */
        public bool AllowMultiHit => allowMultiHit;

        /** Is movement of the character allowed while performing this attack? */
        public bool AllowMovement => allowMovement;

        /** Can this attack be interrupted? */
        public bool CanInterrupt => canInterrupt;

        /**
         * Performs this attack for the provided attacker. This will return a list of the hit enemies.
         * <param name="instigator">The combat controller that is performing this attack.</param>
         * <param name="hits">The characters that were hit by this attack. Do not access until finished.</param>
         * <param name="onProgress">An optional function to call as the attack progresses.</param>
         * <param name="onComplete"> An action performed after execution finishes. </param>
         * <param name="onHit"> An action performed when the attack connects. </param>
         * <returns>The coroutine IEnumerator generated by the action used to cancel the attack if needed.</returns>
         */
        public abstract IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null, Action onHit = null);

        /**
         * Handles cancelling this attack.
         * <param name="instigator">The combat controller that is performing this attack.</param>
         * <param name="coroutine">The coroutine tracked by the controller.</param>
         * <returns>True if the attack could be cancelled.</returns>
         */
        public virtual bool TryCancel(CombatController instigator, IEnumerator coroutine)
        {
            if (!canInterrupt) return false;
            if (null != coroutine)
            {
                instigator.StopCoroutine(coroutine);
            }
            if (!allowMovement) instigator.Character.CharacterMovement.CanMove = true;
            return true;
        }

    }
}