using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class InventoryMenuItemDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text itemDetailsTitle;
        [SerializeField] private TMP_Text itemDetailsDescription;
        [SerializeField] private GameObject itemDetailsEquipOption;
        [SerializeField] private GameObject itemDetailsUnequipOption;
        //[SerializeField] private GameObject itemDetailsDropOption;
        
        /** The inventory menu that owns this object. */
        public InventoryMenu InventoryMenu { get; set; }

        /** The underlying entry data. */
        private InventoryEntry _entry;
        
        /** The entry used by this details panel. */
        public InventoryEntry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                Initialize();
            }
        }

        /**
         * Populates the details screen with the data of the assigned entry.
         */
        private void Initialize()
        {
            if (Entry == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            itemDetailsTitle.text = Entry.Item.DisplayName;
            itemDetailsDescription.text = Entry.Item.Description;
            if (Entry is ComputerPartInstance cp)
            {
                itemDetailsEquipOption.gameObject.SetActive(!cp.Equipped);
                itemDetailsUnequipOption.gameObject.SetActive(cp.Equipped);
            }
            else
            {
                itemDetailsEquipOption.gameObject.SetActive(false);
                itemDetailsUnequipOption.gameObject.SetActive(false);
            }
        }

        /**
         * Equips the selected item.
         */
        public void EquipItem()
        {
            if (Entry is ComputerPartInstance cp)
            {
                InventoryMenu.Player.EquipItem(cp);
                itemDetailsEquipOption.gameObject.SetActive(false);
                itemDetailsUnequipOption.gameObject.SetActive(true);
            }
        }
        
        /**
         * Unequips the selected item.
         */
        public void UnequipItem()
        {
            if (Entry is ComputerPartInstance cp)
            {
                InventoryMenu.Player.UnequipItemSlot(cp.ComputerPartItem.PartType);
                itemDetailsEquipOption.gameObject.SetActive(true);
                itemDetailsUnequipOption.gameObject.SetActive(false);
            }
        }
    }
}