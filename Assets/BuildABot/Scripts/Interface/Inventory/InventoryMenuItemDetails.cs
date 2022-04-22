using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class InventoryMenuItemDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text itemDetailsTitle;
        [SerializeField] private TMP_Text itemDetailsDescription;
        [SerializeField] private Button itemDetailsEquipOption;
        [SerializeField] private Button itemDetailsUnequipOption;
        //[SerializeField] private GameObject itemDetailsDropOption;
        
        /** The inventory menu that owns this object. */
        public InventoryMenu InventoryMenu { get; set; }

        /** The underlying selected slot. */
        private InventoryMenuItemSlot _slot;
        
        /** The entry used by this details panel. */
        public InventoryMenuItemSlot Slot
        {
            get => _slot;
            set
            {
                if (enabled && _slot != null && _slot.Entry.CanEquip)
                {
                    _slot.Entry.OnEquip -= OnEquipped;
                    _slot.Entry.OnUnequip -= OnUnequipped;
                }
                _slot = value;
                if (enabled && _slot != null && _slot.Entry.CanEquip)
                {
                    _slot.Entry.OnEquip += OnEquipped;
                    _slot.Entry.OnUnequip += OnUnequipped;
                }
                Refresh();
            }
        }

        private void OnEnable()
        {
            if (_slot != null && _slot.Entry.CanEquip)
            {
                _slot.Entry.OnEquip += OnEquipped;
                _slot.Entry.OnUnequip += OnUnequipped;
            }
        }

        private void OnDisable()
        {
            if (_slot != null && _slot.Entry.CanEquip)
            {
                _slot.Entry.OnEquip -= OnEquipped;
                _slot.Entry.OnUnequip -= OnUnequipped;
            }
        }

        /**
         * Populates the details screen with the data of the assigned entry.
         */
        public void Refresh()
        {
            if (Slot == null || Slot.Entry == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            itemDetailsTitle.text = Slot.Entry.Item.DisplayName;
            itemDetailsDescription.text = Slot.Entry.Item.Description;
            if (Slot.Entry is ComputerPartInstance cp)
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
            if (Slot.Entry is ComputerPartInstance cp)
            {
                InventoryMenu.Player.EquipItem(cp);
            }
        }
        
        /**
         * Unequips the selected item.
         */
        public void UnequipItem()
        {
            if (Slot.Entry is ComputerPartInstance cp)
            {
                InventoryMenu.Player.UnequipItemSlot(cp.ComputerPartItem.PartType);
            }
        }

        private void OnEquipped(InventoryEntry entry)
        {
            itemDetailsEquipOption.gameObject.SetActive(false);
            itemDetailsUnequipOption.gameObject.SetActive(true);
            Slot.Refresh();
            itemDetailsUnequipOption.Select();
        }

        private void OnUnequipped(InventoryEntry entry)
        {
            itemDetailsEquipOption.gameObject.SetActive(true);
            itemDetailsUnequipOption.gameObject.SetActive(false);
            Slot.Refresh();
            itemDetailsEquipOption.Select();
        }
    }
}