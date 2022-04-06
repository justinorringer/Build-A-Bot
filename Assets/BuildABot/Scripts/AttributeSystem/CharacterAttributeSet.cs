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
        [Tooltip("The standard operating temperature of the character. This is equivalent to minimum health, although temperature may fall below it.")]
        [SerializeField] private FloatAttributeData baseTemperature;
        [Tooltip("The temperature that results in the death of the character.")]
        [SerializeField] private FloatAttributeData maxTemperature;
        [Tooltip("The rate that the character's temperature changes per second. This can be negative to heat up over time.")]
        [SerializeField] private FloatAttributeData coolDownRate;
        
        [Tooltip("A modifier attribute used in calculating changes to the character's temperature from damage.")]
        [SerializeField] private FloatAttributeData coolingFactor;
        [Tooltip("The multiplier applied to all incoming reductions to an item's durability.")]
        [SerializeField] private FloatAttributeData durabilityDegradationRate;
        
        [Tooltip("The character's movement speed.")]
        [SerializeField] private FloatAttributeData movementSpeed;
        [Tooltip("The force that the character jumps with.")]
        [SerializeField] private FloatAttributeData jumpForce;
        [Tooltip("The number of times that a character can jump before having to land.")]
        [SerializeField] private IntAttributeData maxJumpCount;
        [Tooltip("The multiplier applied to jump force after each jump when multi-jumping.")]
        [SerializeField] private FloatAttributeData jumpForceFalloff;
        
        [Tooltip("The current knock-back value.")]
        [SerializeField] private FloatAttributeData knockback;

        [Tooltip("The amount of power behind the character's light attacks.")]
        [SerializeField] private FloatAttributeData lightAttackPower;
        [Tooltip("The amount of power behind the character's medium attacks.")]
        [SerializeField] private FloatAttributeData mediumAttackPower;
        [Tooltip("The amount of power behind the character's heavy attacks.")]
        [SerializeField] private FloatAttributeData heavyAttackPower;
        
        [Tooltip("The number of slots available in the player's inventory.")]
        [SerializeField] private IntAttributeData inventorySpace;
        
        [Tooltip("The view distance multiplier that is used to zoom the camera in or out.")]
        [SerializeField] private FloatAttributeData viewDistance;

        /** The temperature of the character, equivalent to health. */
        public FloatAttributeData Temperature => temperature;
        /** The standard operating temperature of the character. This is equivalent to minimum health, although temperature may fall below it. */
        public FloatAttributeData BaseTemperature => baseTemperature;
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
        /** The force that the character jumps with. */
        public FloatAttributeData JumpForce => jumpForce;
        /** The height that the character can jump. */
        public IntAttributeData MaxJumpCount => maxJumpCount;
        /** The multiplier applied to jump force after each jump when multi-jumping. */
        public FloatAttributeData JumpForceFalloff => jumpForceFalloff;

        public FloatAttributeData Knockback => knockback;

        /** The amount of power behind the character's light attacks. */
        public FloatAttributeData LightAttackPower => lightAttackPower;
        /** The amount of power behind the character's medium attacks. */
        public FloatAttributeData MediumAttackPower => mediumAttackPower;
        /** The amount of power behind the character's heavy attacks. */
        public FloatAttributeData HeavyAttackPower => heavyAttackPower;

        /** The number of slots available in the player's inventory. */
        public IntAttributeData InventorySpace => inventorySpace;

        /** The view distance multiplier that is used to zoom the camera in or out. */
        public FloatAttributeData ViewDistance => viewDistance;
    }
}