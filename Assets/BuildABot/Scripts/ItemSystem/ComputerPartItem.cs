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
        BluetoothDongle,
        WifiDongle,
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

        [Tooltip("The amount that this item protects the user from overheating while equipped. A negative value will cause the user to overheat more easily.")]
        [SerializeField] private float coolingFactor = 0.0f;

        [Tooltip("The list of effects provided by this item when equipped.")]
        [SerializeField] private List<EffectInstance> effects = new List<EffectInstance>();

        /** The type of part that this item is. */
        public EComputerPartSlot PartType => partType;
        
        /** The maximum durability of instances of this item (read-only). */
        public int MaxDurability => maxDurability;
        
        /** The cooling or heating impact of this item when equipped (read-only). */
        public float CoolingFactor => coolingFactor;

        /** The effects used by this item. */
        public List<EffectInstance> Effects => effects;
    }
}