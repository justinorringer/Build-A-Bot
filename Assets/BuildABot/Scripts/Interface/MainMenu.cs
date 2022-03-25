using UnityEngine;

namespace BuildABot
{
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("The player instance using this menu.")]
        [SerializeField] private Player player;
        
        [Tooltip("The inventory menu component used by this menu.")]
        [SerializeField] private InventoryMenu inventoryMenu;
    }
}