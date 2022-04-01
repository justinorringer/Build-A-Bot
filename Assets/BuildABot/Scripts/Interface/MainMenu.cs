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

        protected void OnEnable()
        {
            // Reset the state
            inventoryMenu.gameObject.SetActive(false);
            landingOptions.SetActive(true);
        }
    }
}