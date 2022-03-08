using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{

    /**
     * Represents the duration that an effect is applied for.
     */
    public enum EEffectDurationMode
    {
        /** The effect is applied instantly and immediately removed. This will affect BaseValue. */
        Instant,
        /** The effect is applied for a set duration. This affects CurrentValue. */
        ForDuration,
        /** The effect is applied for an indefinite duration until removed. This affects CurrentValue. */
        UntilRemoved
    }

    /**
     * The way that multiple applications of the same effect will be applied to an attribute set.
     * This does not impact Instant effect instances.
     */
    public enum EEffectStackingMode
    {
        /** The newest effect version replaces the older version. */
        Replace,
        /** Stacks the effects together. */
        Combine,
        /** Keeps the originally applied effect and ignores the newly applied version. */
        KeepOriginal,
        /** Resets the timer for the originally applied effect. Only used with ForDuration effects. */
        Reset,
        /** Keeps the application instance that had the highest applied magnitude. If equal, the newer instance is kept. */
        KeepHighestMagnitude
    }
    
    /**
     * An effect that can be applied to an attribute set to modify its attribute values.
     */
    [CreateAssetMenu(fileName = "NewEffect", menuName = "Build-A-Bot/Effect", order = 3)]
    public class Effect : ScriptableObject
    {
        [Tooltip("The name of this effect displayed in game.")]
        [SerializeField] private string displayName;
        [Tooltip("The description of this effect displayed in game.")]
        [Multiline]
        [SerializeField] private string description;
        
        [Tooltip("The duration mode of this effect.")]
        [SerializeField] private EEffectDurationMode durationMode;
        
        [Tooltip("[CURRENTLY UNUSED] The stacking mode of this effect.")]
        [SerializeField] private EEffectStackingMode stackingMode;

        [Tooltip("The time that this effect will stay active for if using the ForDuration duration mode.")]
        [Min(0.0f)]
        [SerializeField] private float duration;

        /** The attribute set type targeted by this effect. */
        [SerializeField] private AttributeSetSelector target;

        /** The internal list of modifiers used by this effect. */
        [SerializeReference] private List<AttributeModifierBase> modifiers;

        /** The name of this effect displayed in game. */
        public string DisplayName => displayName;
        /** The description of this effect displayed in game. */
        public string Description => description;

        /** The selector used by this effect to determine the target attribute type. */
        public AttributeSetSelector Target => target;
        
        /** The duration mode used by this effect. */
        public EEffectDurationMode DurationMode => durationMode;

        /** The stacking mode used by this effect. */
        public EEffectStackingMode StackingMode => stackingMode;

        /** The list of modifiers used by this effect. */
        public List<AttributeModifierBase> Modifiers => modifiers;

        /** The duration this effect will be applied for if applicable. */
        public float Duration => EEffectDurationMode.ForDuration == durationMode ? duration : 0;
    }
}