using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    /**
     * A repair tool can be considered the equivalent of a healing item or consumable.
     */
    [CreateAssetMenu(fileName = "NewRepairTool", menuName = "Build-A-Bot/Item/Repair Tool", order = 1)]
    public class RepairToolItem : StackableItem
    {
        public override EItemType Type => EItemType.RepairTool;

        [Tooltip("The list of effects applied by this repair tool when used.")]
        [SerializeField] private List<EffectInstance> effects = new List<EffectInstance>();

        /** The effects applied by this item. */
        public List<EffectInstance> Effects => effects;
    }
}