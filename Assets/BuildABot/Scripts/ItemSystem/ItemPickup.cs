using System;
using UnityEngine;

namespace BuildABot
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private Item item;
        [SerializeField] private int count = 1;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Item Item
        {
            get => item;
            set
            {
                item = value;
                Initialize();
            }
        }

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
            }
            else
            {
                spriteRenderer.sprite = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                if (player.Inventory.TryAddItem(Item, Count, out int overflow))
                {
                    Destroy(gameObject);
                }
                else
                {
                    count = overflow;
                }
            }
        }

        private void OnValidate()
        {
            Initialize();
        }
    }
}