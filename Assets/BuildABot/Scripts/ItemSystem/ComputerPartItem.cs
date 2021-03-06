using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{

    /**
     * The types of computer parts that can be equipped by the player.
     */
    public enum EComputerPartSlot
    {
        CPU,
        Motherboard,
        GPU,
        PowerSupply,
        Case,
        RAM,
        HardDrive,
        SSD,
        DiskDrive,
        Fan,
        Lights,
        Keyboard,
        Mouse,
        Monitor,
        FlashDrive,
        CD,
        Cable,
        Microphone,
        Speaker,
        WirelessCard,
        SDCard,
        SDReader,
        Printer
    }

    /**
     * A computer part item that acts as a piece of equipment for the player. Computer parts do not stack and can break.
     */
    [CreateAssetMenu(fileName = "NewComputerPart", menuName = "Build-A-Bot/Item/Computer Part", order = 0)]
    public class ComputerPartItem : Item
    {
        public override EItemType Type => EItemType.ComputerPart;

        [Tooltip("The type computer part that this item represents.")]
        [SerializeField] private EComputerPartSlot partType;
        
        [Tooltip("The max durability available to instances of this item.")]
        [Min(1)]
        [SerializeField] private int maxDurability = 1;
        
        [Tooltip("The quality level of this item.")]
        [Min(1)]
        [SerializeField] private int quality = 1;

        [Tooltip("The list of effects provided by this item when equipped.")]
        [SerializeField] private List<EffectInstance> effects = new List<EffectInstance>();
        
        [Tooltip("The attack provided by this attack.")]
        [HideInInspector]
        [SerializeField] private AttackData attack;

        /** The type of part that this item is. */
        public EComputerPartSlot PartType => partType;
        
        /** The maximum durability of instances of this item (read-only). */
        public int MaxDurability => maxDurability;

        /** The quality level of this item. */
        public int Quality => quality;

        /** The effects used by this item. */
        public List<EffectInstance> Effects => effects;

        /** The attack provided by this attack. */
        public AttackData Attack => attack;
    }
}