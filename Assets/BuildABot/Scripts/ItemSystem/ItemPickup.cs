using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemPickup : MonoBehaviour
    {
        [Tooltip("The item being picked up.")]
        [SerializeField] private Item item;
        [Tooltip("The count of the item in this pickup.")]
        [SerializeField] private int count = 1;
        
        [Tooltip("The sprite renderer used to display the item.")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Tooltip("An event fired when this item is picked up.")]
        [SerializeField] private UnityEvent<Player> onPickup;

        /** The item being picked up. */
        public Item Item
        {
            get => item;
            set
            {
                item = value;
                Initialize();
            }
        }

        /** The count of the item in this pickup. */
        public int Count
        {
            get => count;
            set => count = value < 1 ? 1 : value;
        }

        protected void Awake()
        {
            Initialize();
        }

        protected void Initialize()
        {
            if (Item != null)
            {
                spriteRenderer.sprite = item.OverworldSprite;
                spriteRenderer.color = item.SpriteTint;
            }
            else
            {
                spriteRenderer.sprite = null;
                spriteRenderer.color = Color.white;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                if (player.Inventory.TryAddItem(Item, Count, out int overflow))
                {
                    onPickup.Invoke(player);
                    Destroy(gameObject);
                }
                else
                {
                    count = overflow;
                }
            }
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            EditorApplication.delayCall += OnValidateImpl;
        }

        /**
         * The actual implementation of OnValidate, called with a delay to avoid warnings in the inspector.
         */
        private void OnValidateImpl()
        {
            EditorApplication.delayCall -= OnValidateImpl;
            if (this == null) return;
            Initialize();
        }
    #endif
    }
}