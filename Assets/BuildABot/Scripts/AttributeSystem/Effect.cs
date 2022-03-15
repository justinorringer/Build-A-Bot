using System.Collections.Generic;
using System.Globalization;
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
        [TextArea]
        [SerializeField] private string description;
        
        [Tooltip("The duration mode of this effect.")]
        [SerializeField] private EEffectDurationMode durationMode;
        
        [Tooltip("The stacking mode of this effect.")]
        [SerializeField] private EEffectStackingMode stackingMode;

        [Tooltip("The time that this effect will stay active for if using the ForDuration duration mode.")]
        [Min(0.0f)]
        [SerializeField] private float duration;

        /** The attribute set type targeted by this effect. */
        [SerializeField] private AttributeSetSelector target;

        [Tooltip("The default magnitude to use when applying instances of this effect. This will be multiplied with any magnitude specified at application time.")]
        [SerializeField] private float baseMagnitude = 1f;

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

        /** The base magnitude applied to all applications of this effect. */
        public float BaseMagnitude => baseMagnitude;

        /** The list of modifiers used by this effect. */
        public List<AttributeModifierBase> Modifiers => modifiers;

        /** The duration this effect will be applied for if applicable. */
        public float Duration => EEffectDurationMode.ForDuration == durationMode ? duration : 0;

        /**
         * Replaces valid placeholder tokens in an effect's description with the correct value.
         * <remarks>
         * The following are always valid tokens: {DURATION}, {NAME}, {MODIFIER_OPERATION[i]}, {MODIFIER_TARGET[i]},
         * {MODIFIER_TARGET_LOWER[i]} where i is the index of a modifier in the Effect.
         *
         * Additionally, the following modifiers are available for modifier indices with constant value providers:
         * {MODIFIER_VALUE[i]}, {MODIFIER_VALUE_NO_MAG[i]}, and {MODIFIER_VALUE_BASE_MAG_ONLY[i]}
         * </remarks>
         * <param name="effect">The effect whose description is being displayed.</param>
         * <param name="magnitude">The magnitude to apply to the effect. Defaults to 1f.</param>
         * <param name="useRichText">Should rich text tags be automatically applied to the result? Defaults to false.</param>
         * <returns>The version of the effect's description with all placeholders replaced with their true values.</returns>
         */
        public static string ApplyDescriptionPlaceholders(Effect effect, float magnitude = 1f, bool useRichText = true)
        {
            string result = effect.description;
            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "{DURATION}", effect.duration.ToString(CultureInfo.InvariantCulture) },
                { "{NAME}", effect.displayName }
            };
            for (int i = 0; i < effect.modifiers.Count; i++)
            {
                AttributeModifierBase mod = effect.modifiers[i];
                tokens.Add($"{{MODIFIER_OPERATION[{i}]}}", mod.OperationType.ToString());

                AttributeModifier<float> floatMod = mod as AttributeModifier<float>;
                AttributeModifier<int> intMod = mod as AttributeModifier<int>;
                bool isFloat = floatMod != null;
                bool isInt = intMod != null;

                string targetStr =
                    (isFloat
                        ? floatMod.Attribute.SelectedAttributeNameFriendly
                        : intMod?.Attribute.SelectedAttributeNameFriendly)
                    ?? (useRichText ? "<b>[invalid modifier]</b>" : "[invalid modifier]");
                
                tokens.Add($"{{MODIFIER_TARGET_LOWER[{i}]}}", targetStr.ToLower());
                tokens.Add($"{{MODIFIER_TARGET[{i}]}}", targetStr);
                
                switch (mod.ValueProviderType)
                {
                    case EAttributeModifierValueProviderType.Constant:
                        float floatValue = isFloat ? floatMod.GetModifierValue(null, null) : 0;
                        float intValue = isInt ? intMod.GetModifierValue(null, null) : 0;
                        tokens.Add($"{{MODIFIER_VALUE[{i}]}}",
                            useRichText ?
                            $"<b>{(isFloat ? (floatValue * magnitude * effect.baseMagnitude) : ((int)(intValue * magnitude * effect.baseMagnitude)))}</b>" :
                            $"{(isFloat ? (floatValue * magnitude * effect.baseMagnitude) : ((int)(intValue * magnitude * effect.baseMagnitude)))}");
                        tokens.Add($"{{MODIFIER_VALUE_BASE_MAG_ONLY[{i}]}}",
                            useRichText ?
                                $"<b>{(isFloat ? (floatValue * effect.baseMagnitude) : ((int)(intValue * effect.baseMagnitude)))}</b>" :
                                $"{(isFloat ? (floatValue * effect.baseMagnitude) : ((int)(intValue * effect.baseMagnitude)))}");
                        tokens.Add($"{{MODIFIER_VALUE_NO_MAG[{i}]}}",
                            useRichText ?
                                $"<b>{(isFloat ? floatValue : intValue)}</b>" :
                                $"{(isFloat ? floatValue : intValue)}");
                        break;
                    case EAttributeModifierValueProviderType.Attribute:
                        break;
                }
            }
            // Replace tokens
            foreach (var entry in tokens)
            {
                result = result.Replace(entry.Key, entry.Value);
            }

            return result;
        }
    }
}