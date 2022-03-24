using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * The core data and logic associated with the Player character.
     */
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerInput))]
    public class Player : Character
    {

        /** The player movement component used by this player. */
        private PlayerMovement _playerMovement;
        /** The player input component used by this player. */
        private PlayerInput _playerInput;

        /** The player input component used by this player. */
        public PlayerInput PlayerInput => _playerInput;

        public override CharacterMovement CharacterMovement => _playerMovement;

#region Item and Equipment Handling
        
        /**
         * The data stored in a single equipment slot.
         */
        private struct EquipmentSlotData
        {
            /** The item instance that is equipped. */
            public ComputerPartInstance Item;
            /** The list of effects being applied by the item that must be removed when unequipped. */
            public List<AttributeSet.AppliedEffectHandle> AppliedEffects;
        }

        /** The table of equipment slots and their stored data. */
        private readonly Dictionary<EComputerPartSlot, EquipmentSlotData> _equippedItems =
            new Dictionary<EComputerPartSlot, EquipmentSlotData>();
        
        // TODO: Add events for when items are equipped and unequipped

        /**
         * Gets the computer part equipped to the specified slot. If nothing is equipped, null is returned.
         * <param name="slot">The type of item slot to check.</param>
         * <returns>The equipped item or null if nothing is equipped.</returns>
         */
        public ComputerPartInstance GetItemEquippedToSlot(EComputerPartSlot slot)
        {
            if (_equippedItems.TryGetValue(slot, out EquipmentSlotData data))
            {
                return data.Item;
            }
            return null;
        }

        /**
         * Equips the provided item to the player. If an item is already equipped to it's slot, it will be unequipped.
         * <param name="item">The item to equip.</param>
         */
        public void EquipItem(ComputerPartInstance item)
        {
            ComputerPartItem baseItem = item.Item as ComputerPartItem;
            Debug.Assert(baseItem != null, 
                "The base item of a ComputerPartInstance should never be null or a non-ComputerPartItem instance.");
            EComputerPartSlot slot = baseItem.PartType;
            
            // Unequip the slots previous item if one exists
            if (_equippedItems.TryGetValue(slot, out _)) UnequipItemSlot(slot);

            // Create the slot data
            EquipmentSlotData data = new EquipmentSlotData()
            {
                Item = item,
                AppliedEffects = new List<AttributeSet.AppliedEffectHandle>()
            };

            // Apply effects
            foreach (EffectInstance effectInstance in baseItem.Effects)
            {
                // Ensure that the effect has the proper target
                if (effectInstance.effect.Target.SelectedType == typeof(CharacterAttributeSet))
                {
                    // Apply the effect
                    AttributeSet.AppliedEffectHandle handle = Attributes.ApplyEffect(effectInstance, this);
                    
                    // When the item is unequipped, any "UntilRemoved" effects will be as well
                    if (effectInstance.effect.DurationMode == EEffectDurationMode.UntilRemoved) data.AppliedEffects.Add(handle);
                }
            }
            
            // Store the slot entry and mark as equipped
            _equippedItems.Add(slot, data);
            item.Equipped = true;
        }

        /**
         * Unequips the item from the provided slot if one is currently equipped.
         * <param name="slot">The slot to remove any equipped items from.</param>
         */
        public void UnequipItemSlot(EComputerPartSlot slot)
        {
            if (_equippedItems.TryGetValue(slot, out EquipmentSlotData data))
            {
                foreach (AttributeSet.AppliedEffectHandle effect in data.AppliedEffects)
                {
                    Attributes.RemoveEffect(effect);
                }
                // Remove the data and mark as not equipped
                _equippedItems.Remove(slot);
                data.Item.Equipped = false;
            }
        }
        
#endregion

#region Follow Mouse Debug tool

        [Header("Debug")]
        [SerializeField] private bool useFollowMouseTool;

        private Vector2 _target;
        private Camera _mainCamera;
        
#endregion

        protected override void Awake()
        {
            base.Awake();
            Attributes.Initialize();
        }
        
        
        protected void Start()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _playerInput = GetComponent<PlayerInput>();

            if (useFollowMouseTool)
            {
                _playerInput.InputEnabled = false;
                _playerMovement.ChangeMovementMode(ECharacterMovementMode.Flying);
                _mainCamera = Camera.main;
            }
        }

        protected void Update()
        {
            if (useFollowMouseTool)
            {
                _playerMovement.MoveToPosition(_target);
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = _mainCamera.nearClipPlane;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
                _target = new Vector2(worldPos.x, worldPos.y);
            }
        }

    }
}