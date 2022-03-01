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
    }
}