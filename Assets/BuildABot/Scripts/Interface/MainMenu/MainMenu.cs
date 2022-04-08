using System;
using UnityEngine;

namespace BuildABot
{
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("The player instance using this menu.")]
        [SerializeField] private Player player;
        
        [Tooltip("The inventory menu component used by this menu.")]
        [SerializeField] private InventoryMenu inventoryMenu;

        [Tooltip("The object that holds the landing options menu.")]
        [SerializeField] private GameObject landingOptions;

        /** The player that owns this menu. */
        public Player Player => player;

        protected void OnEnable()
        {
            // Reset the state
            inventoryMenu.gameObject.SetActive(false);
            landingOptions.SetActive(true);
        }

        /**
         * Returns the menu to the starting landing page.
         */
        public void ReturnToLandingPage()
        {
            // Reset the state
            inventoryMenu.gameObject.SetActive(false);
            landingOptions.SetActive(true);
        }

        /**
         * Opens the menu straight to the inventory.
         */
        public void OpenInventory()
        {
            inventoryMenu.gameObject.SetActive(true);
            landingOptions.SetActive(false);
        }
    }
}