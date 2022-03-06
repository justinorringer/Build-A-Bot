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
     * An effect that can be applied to an attribute set to modify its attribute values.
     */
    [CreateAssetMenu(fileName = "NewEffect", menuName = "Build-A-Bot/Effect", order = 0)]
    public class Effect : ScriptableObject
    {
        [Tooltip("The name of this effect displayed in game.")]
        [SerializeField] private string displayName;
        [Tooltip("The description of this effect displayed in game.")]
        [Multiline]
        [SerializeField] private string description;
        
        [Tooltip("The duration mode of this effect.")]
        [SerializeField] private EEffectDurationMode durationMode;

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

        /** The list of modifiers used by this effect. */
        public List<AttributeModifierBase> Modifiers => modifiers;

        /** The duration this effect will be applied for if applicable. */
        public float Duration => EEffectDurationMode.ForDuration == durationMode ? duration : 0;
    }
}