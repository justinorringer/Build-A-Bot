using UnityEngine;

namespace BuildABot
{
    /**
     * An item required for story progression or that is otherwise expected to always be held by the player.
     */
    [CreateAssetMenu(fileName = "NewKeyItem", menuName = "Build-A-Bot/Item/Key Item", order = 2)]
    public class KeyItem : Item
    {
        public override EItemType Type => EItemType.KeyItem;

        public override bool Sellable => false;

        public override bool Removable => false;
    }
}