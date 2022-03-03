using System;
using UnityEngine;

namespace BuildABot
{
    /**
     * The attributes common to all characters in the game.
     */
    [Serializable]
    public class CharacterAttributeSet : AttributeSet
    {
        [Tooltip("The temperature of the character, equivalent to health.")]
        [SerializeField] private FloatAttributeData temperature;
        [Tooltip("The temperature that results in the death of the character.")]
        [SerializeField] private FloatAttributeData maxTemperature;
        [Tooltip("the rate that the character's temperature changes per second. This can be negative to heat up over time.")]
        [SerializeField] private FloatAttributeData coolDownRate;
        
        [Tooltip("A modifier attribute used in calculating changes to the character's temperature from damage.")]
        [SerializeField] private FloatAttributeData coolingFactor;
        [Tooltip("The multiplier applied to all incoming reductions to an item's durability.")]
        [SerializeField] private FloatAttributeData durabilityDegradationRate;
        
        [Tooltip("The character's movement speed.")]
        [SerializeField] private FloatAttributeData movementSpeed;
        [Tooltip("The height that the character can jump.")]
        [SerializeField] private FloatAttributeData jumpHeight;
        [Tooltip("The number of times that a character can jump before having to land.")]
        [SerializeField] private IntAttributeData maxJumpCount;
        
        [Tooltip("The amount of power behind the character's light attacks.")]
        [SerializeField] private FloatAttributeData lightAttackPower;
        [Tooltip("The amount of power behind the character's medium attacks.")]
        [SerializeField] private FloatAttributeData mediumAttackPower;
        [Tooltip("The amount of power behind the character's heavy attacks.")]
        [SerializeField] private FloatAttributeData heavyAttackPower;
        
        [Tooltip("The number of slots available in the player's inventory.")]
        [SerializeField] private IntAttributeData inventorySpace;

        /** The temperature of the character, equivalent to health. */
        public FloatAttributeData Temperature => temperature;
        /** The temperature that results in the death of the character. */
        public FloatAttributeData MaxTemperature => maxTemperature;
        /** The amount that the character's temperature changes per second. */
        public FloatAttributeData CoolDownRate => coolDownRate;
        
        /** A modifier attribute used in calculating changes to the character's temperature. */
        public FloatAttributeData CoolingFactor => coolingFactor;
        /** The multiplier applied to all incoming reductions to an item's durability. */
        public FloatAttributeData DurabilityDegradationRate => durabilityDegradationRate;


        /** The character's movement speed. */
        public FloatAttributeData MovementSpeed => movementSpeed;
        /** The height that the character can jump. */
        public FloatAttributeData JumpHeight => jumpHeight;
        /** The height that the character can jump. */
        public IntAttributeData MaxJumpCount => maxJumpCount;

        /** The amount of power behind the character's light attacks. */
        public FloatAttributeData LightAttackPower => lightAttackPower;
        /** The amount of power behind the character's medium attacks. */
        public FloatAttributeData MediumAttackPower => mediumAttackPower;
        /** The amount of power behind the character's heavy attacks. */
        public FloatAttributeData HeavyAttackPower => heavyAttackPower;

        /** The number of slots available in the player's inventory. */
        public IntAttributeData InventorySpace => inventorySpace;
    }
}