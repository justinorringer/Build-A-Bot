using UnityEngine;

namespace BuildABot
{
    
    /**
     * A miscellaneous item with no purpose other than to be collected by the player and/or sold to the trader.
     */
    [CreateAssetMenu(fileName = "NewMiscItem", menuName = "Build-A-Bot/Item/Misc Item", order = 3)]
    public class MiscItem : StackableItem
    {
        public override EItemType Type => EItemType.Misc;
    }
}